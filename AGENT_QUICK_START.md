# Quick Start: Using Optimized Agents

This guide shows you how to use the newly optimized agent structure for maximum productivity.

## 🚀 Quick Start (30 seconds)

### For GitHub Copilot Users

1. **Invoke an agent** in your GitHub Copilot chat:
   ```
   @Backend Agent help me implement a Product entity with CQRS
   ```

2. **That's it!** The agent will:
   - Use the optimized prompt from `.github/agents/`
   - Reference detailed docs from `docs/` as needed
   - Provide focused, actionable guidance

### For Manual Development

1. **Check AGENTS.md** to find the right agent for your task
2. **Read the agent file** in `.github/agents/` for quick overview
3. **Dive into docs** in `docs/` folder for comprehensive guidelines

## 📁 Where to Find Things

```
.github/agents/        ← Quick agent prompts (2-3KB each)
docs/                  ← Detailed documentation (8-63KB each)
AGENTS.md              ← Central catalog & usage guide
```

## 🎯 Common Tasks

### Task: Implement a New Backend Entity

1. **Invoke**: `@Backend Agent`
2. **Quick ref**: `.github/agents/Backend Agent.agent.md`
3. **Deep dive**: `docs/backend-agent.md`
4. **Example**: `docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md`

### Task: Create a Frontend Page

1. **Invoke**: `@Frontend Agent`
2. **Quick ref**: `.github/agents/Frontend Agent.agent.md`
3. **Deep dive**: `docs/frontend-agent.md`
4. **Example**: `docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md`

### Task: Write Unit Tests

1. **Invoke**: `@Backend Unit Testing Agent`
2. **Quick ref**: `.github/agents/Backend Unit Testing Agent.agent.md`
3. **Deep dive**: `docs/backend-unit-testing-agent.md`

### Task: Design User Experience

1. **Invoke**: `@UX Agent`
2. **Quick ref**: `.github/agents/UX Agent.agent.md`
3. **Deep dive**: `docs/ux-agent.md`

## 💡 Pro Tips

### When to Use Which Resource

| Need | Use |
|------|-----|
| Quick task | Agent file in `.github/agents/` |
| Comprehensive guide | Docs in `docs/` folder |
| Code examples | Reference implementations in `docs/` |
| Find an agent | `AGENTS.md` catalog |

### Combining Agents

For full-stack features, use multiple agents:

```
1. @UX Agent - Define the user experience
2. @Backend Agent - Implement backend API
3. @Frontend Agent - Build the UI
4. @Backend Unit Testing Agent - Write tests
```

### Understanding Agent Boundaries

Each agent has a specific domain:
- **Don't ask Backend Agent about UI** → Use Frontend Agent
- **Don't ask Frontend Agent about database** → Use Backend Agent
- **Cross-domain tasks** → Use multiple agents in sequence

## 📊 Benefits You Get

### ⚡ Performance
- **87% smaller** agent files
- **Faster** agent responses
- **Reduced** token usage

### 🔍 Discoverability
- **Auto-discovered** by GitHub Copilot
- **Clear naming** for easy selection
- **Centralized catalog** in AGENTS.md

### 📚 Flexibility
- **Quick prompts** when you need speed
- **Detailed docs** when you need depth
- **Both available** at your fingertips

## 🆘 Need Help?

1. **Start with AGENTS.md** - Complete catalog and guide
2. **Check agent file** - Quick overview and constraints
3. **Read documentation** - Comprehensive implementation guide
4. **See examples** - WAREHOUSE_IMPLEMENTATION_REFERENCE.md

## 🔄 Agent Workflow

```
┌─────────────────┐
│   Your Task     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Select Agent   │  ← Check AGENTS.md
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Read Prompt    │  ← .github/agents/*.agent.md
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Get Details    │  ← docs/*.md (if needed)
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Implement     │  ← Follow patterns
└─────────────────┘
```

## 🎓 Learn More

- **AGENTS.md** - Complete agent catalog and documentation
- **AGENT_MIGRATION_SUMMARY.md** - Technical details about the migration
- **docs/WAREHOUSE_IMPLEMENTATION_REFERENCE.md** - Canonical implementation example

---

**Ready to start?** Choose your agent and get building! 🚀
