import sys

with open('ImTK/Core/ImTKApplication.cs', 'r') as f:
    content = f.read()

start_index = content.find('private static void ProcessPendingQueuesAndStateChanges()')
end_index = content.find('}\n    }\n}')

with open('patch.cs', 'r') as f:
    patch = f.read()

new_content = content[:start_index] + patch + '        }\n    }\n}\n'

with open('ImTK/Core/ImTKApplication.cs', 'w') as f:
    f.write(new_content)
