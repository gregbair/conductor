# Conductor

An ansible-compatible orchestrator written in C#.

## Aims

Conductor aims to be a drop-in replacement for Ansible, while being faster, more efficient with resources, and more extendable.

On the roadmap:

- Using Conductor as a library in .NET programs
- Using Conductor with Ansible YAML files
- Modules:
  - Systemd
  - podman
  - package/dnf/apt
  - shell commands (like Ansible's command module)
  - template (jinja2, liquid, scriban)