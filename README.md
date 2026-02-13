# HOUDAIFA - Espace Personnel EAGLES

Depot personnel de **HOUDAIFA** pour le projet **RH-OptimERP**.

## Configuration Claude Code

Pour configurer Claude Code avec le framework EAGLES (15 agents, 35 skills, 10 rules, 5 hooks, 6 MCP servers):

```powershell
# 1. Cloner la config partagee
git clone https://github.com/ERP-CORE-DEV/eagles-claude-config.git
cd eagles-claude-config

# 2. Lancer le script d'installation
.\setup_claude.ps1

# 3. Authentification Claude AI
claude auth login

# 4. Cloner le MCPs repo
git clone https://github.com/ERP-CORE-DEV/rh-optimerp-mcps.git C:\RH-OptimERP\MCPs
cd C:\RH-OptimERP\MCPs
# Build each MCP server: npm install && npm run build

# 5. Ouvrir votre microservice dans VS Code avec Claude Code
```

## Pre-requis

- **Windows 11** avec PowerShell 5.1+
- **Node.js 20+** et npm
- **Git** configure avec acces GitHub
- **Claude Code CLI** (`npm install -g @anthropic-ai/claude-code`)
- **GITHUB_PERSONAL_ACCESS_TOKEN** defini dans les variables d'environnement systeme

## Mise a jour

```powershell
cd eagles-claude-config
git pull
.\setup_claude.ps1 -Update
```

## Liens utiles

- [Config partagee EAGLES](https://github.com/ERP-CORE-DEV/eagles-claude-config)
- [MCPs partages](https://github.com/ERP-CORE-DEV/rh-optimerp-mcps)
- [Microservice Sourcing](https://github.com/ERP-CORE-DEV/rh-optimerp-sourcing-candidate-attraction)
