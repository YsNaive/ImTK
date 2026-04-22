using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ImTK;

public static class DrawerBindingExtensions
{
    /// <summary>
    /// Binds the drawer to a property or field using an expression tree.
    /// This automatically sets up the getter and setter delegates for two-way synchronization.
    /// Optionally enables autoSync to pull the value from the source every frame.
    /// </summary>
    /// <typeparam name="TDrawer">The type of the drawer.</typeparam>
    /// <typeparam name="TValue">The type of the value being bound.</typeparam>
    /// <param name="drawer">The drawer instance.</param>
    /// <param name="memberExpression">An expression that returns the target property or field (e.g. () => myObject.MyProperty).</param>
    /// <param name="autoSync">If true, the drawer will automatically pull updates from the source every frame.</param>
    /// <returns>The drawer instance for fluent chaining.</returns>
    public static TDrawer Bind<TDrawer, TValue>(
        this TDrawer drawer,
        Expression<Func<TValue>> memberExpression,
        bool autoSync = true) where TDrawer : RuntimeDrawer<TValue>
    {
        if (memberExpression == null) throw new ArgumentNullException(nameof(memberExpression));

        var body = memberExpression.Body as MemberExpression;
        if (body == null)
        {
            throw new ArgumentException("Expression must be a member expression (property or field access).", nameof(memberExpression));
        }

        var memberInfo = body.Member;
        var targetObjExpression = body.Expression;

        // Compile a delegate to evaluate the target object instance (e.g. evaluating "myObject")
        object targetObj = null;
        if (targetObjExpression != null)
        {
            var targetLambda = Expression.Lambda(targetObjExpression);
            var targetDelegate = targetLambda.Compile();
            targetObj = targetDelegate.DynamicInvoke();
        }

        // Setup Getter
        drawer.getter = memberExpression.Compile();

        // Setup Setter
        if (memberInfo is PropertyInfo propInfo && propInfo.CanWrite)
        {
            drawer.setter = (val) => propInfo.SetValue(targetObj, val);
        }
        else if (memberInfo is FieldInfo fieldInfo && !fieldInfo.IsInitOnly)
        {
            drawer.setter = (val) => fieldInfo.SetValue(targetObj, val);
        }
        else
        {
            // Read-only member, leave setter null or throw?
            // Leaving null is safer, it just won't write back.
            drawer.setter = null;
        }

        drawer.autoSync = autoSync;

        // Initialize UI value immediately
        if (drawer.getter != null)
        {
            drawer.SetValueWithoutNotify(drawer.getter());
        }

        return drawer;
    }
}
