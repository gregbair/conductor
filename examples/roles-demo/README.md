# Conductor Roles Examples

This directory contains example playbooks demonstrating Conductor's role support.

## Role Structure

Roles follow the Ansible directory structure:

```
roles/
└── rolename/
    ├── tasks/
    │   └── main.yml      (required - tasks to execute)
    ├── defaults/
    │   └── main.yml      (optional - default variables, lowest precedence)
    └── vars/
        └── main.yml      (optional - role variables, higher precedence)
```

## Examples

### 01-basic-roles.yml
The simplest way to use roles - just list them by name. Roles are executed before tasks in the play.

```bash
# Run this example
conductor-playbook 01-basic-roles.yml
```

### 02-roles-with-parameters.yml
Pass custom variables to roles using the `vars:` key. These parameters override the role's defaults and vars.

### 03-variable-precedence.yml
Demonstrates variable precedence order:
- **Play vars** (highest precedence)
- **Role parameters** (passed via `vars:`)
- **Role vars** (from `vars/main.yml`)
- **Role defaults** (from `defaults/main.yml`, lowest precedence)

### 04-conditional-roles.yml
Use `when:` conditions to control whether roles are executed based on variables or expressions.

## Variable Precedence

Understanding variable precedence is important when using roles:

1. **Play-level vars** - Defined in the play's `vars:` section, these have the highest precedence
2. **Role parameters** - Variables passed to the role via `vars:` in the role definition
3. **Role vars** - Variables defined in `roles/rolename/vars/main.yml`
4. **Role defaults** - Variables defined in `roles/rolename/defaults/main.yml` (lowest precedence)

Example:
```yaml
- name: My play
  vars:
    port: 8080  # Highest precedence
  roles:
    - name: webserver
      vars:
        port: 8000  # This will be overridden by play vars
```

In this example, the webserver role will see `port: 8080` (from play vars), not `port: 8000`.

## Running the Examples

From the examples/roles-demo directory:

```bash
# View the playbook
cat 01-basic-roles.yml

# Run with conductor-playbook (when CLI is ready)
conductor-playbook 01-basic-roles.yml

# Check the roles directory
ls -la roles/
```

## Role Features

Currently supported:
- ✅ External roles (loaded from `./roles/` directory)
- ✅ Multiple roles per play
- ✅ Role parameters (passing variables)
- ✅ Variable precedence (defaults < vars < play vars)
- ✅ Conditional execution (`when:` conditions)
- ✅ Role execution before tasks
- ✅ Tags on roles

Future enhancements:
- 🔄 `import_role` / `include_role` as tasks
- 🔄 Handlers
- 🔄 Templates and files
- 🔄 Role dependencies (meta/main.yml)
- 🔄 Inline role definitions in playbooks
