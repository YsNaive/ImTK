# Naming Conventions

This project enforces a specific set of naming conventions designed to blend standard C++ practices with the VEX Official API style (which heavily relies on `camelCase`).

| Element Type (元素類型) | Naming Rule (命名規則) | Example (範例) |
| :--- | :--- | :--- |
| **Class / Struct (類別與結構體)** | `PascalCase` | `PidController`, `RobotConfig` |
| **Interface (介面)** | `I` + `PascalCase` | `IMotor`, `ILocator` |
| **Module Namespace (模組化命名空間)** | `PascalCase` | `gcvex::Debug`, `gcvex::Application` |
| **Category Namespace (分類用命名空間)** | `snake_case` | `math_utils`, `robotic` |
| **Enum Type (列舉型別)** | `PascalCase` | `DriveMode`, `ProfileState` |
| **Enum Value (列舉值)** | `PascalCase` | `Auto`, `Teleop` |
| **Public Function / Method (公開函式與方法)** | `camelCase` | `calculateOutput()`, `reset()` |
| **Private / Protected Method (私有/保護方法)** | `snake_case` | `calculate_internal()`, `check_done()` |
| **Public Property (公開成員變數)** | `camelCase` | `maxVoltage`, `targetDistance` |
| **Private / Protected Property (私有/保護變數)**| `m_` + `camelCase` | `m_integral`, `m_prevError` |
| **Private Static Property (靜態私有變數)** | `s_` + `camelCase` | `s_instanceCount` |
| **Global / Local Variable (區域/全域變數)** | `camelCase` | `targetValue`, `leftMotor` |
| **Physical Quantity with Unit (含單位的變數)**| `_{unit}` Suffix | `deltaTime_ms`, `targetDistance_in`, `angle_rad` |
| **Constant / Macro (常數與巨集)** | `UPPER_SNAKE_CASE` | `MAX_SPEED_RPM`, `PI` |

### Key Takeaways:
1. **Public Methods and Variables:** We follow the VEX API `camelCase` convention (e.g., `motor.spinFor(...)`, `pid.calculateOutput(...)`) for public API endpoints. However, to explicitly differentiate internal implementations, **Private and Protected Methods** use `snake_case` (e.g., `update_internal()`).
2. **Global Variables:** Global hardware definitions (like `leftMotor`) do not require prefixes like `g_` as they are ubiquitous in robotics control logic.
3. **Module Namespaces:** Namespaces that act as static classes or standalone subsystems (e.g., `gcvex::Debug`) use `PascalCase` to highlight their systemic importance, differentiating them from mere categorizational namespaces (e.g., `math_utils`).
4. **Interfaces:** Pure virtual base classes must be prefixed with `I` (e.g., `ILocator`).
5. **Unit Suffixes:** Any variable representing a physical quantity (time, distance, angle) **must** append a clear `_{unit}` suffix (e.g., `_ms`, `_in`, `_rad`). This applies to private members as well (e.g., `m_timeout_ms`). This overrides pure `camelCase` endings to guarantee physics safety.
