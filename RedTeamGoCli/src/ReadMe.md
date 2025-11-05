# RedTeamGo Cli

A set of tools making Go development easier.
# Commands

This directory contains all CLI commands for the RedTeamGoCli application. Commands are organized into subcommands based on their functionality.

## Command Structure

Commands follow a hierarchical structure: `<subcommand> <command> [options]`

## Available Commands

### Git Subcommand (`git`)

A collection of git-related commands for managing repositories, commits, and pull requests.

#### `git cherry-pick`

**File:** `CherryPickCommand.cs`

Cherry-picks commits by author within a specified commit range or date range. Excludes merge commits and applies changes individually.

**Options:**
- `--author` (required) - Author name or email to filter commits by
- `--start` - Starting commit hash
- `--end` - Ending commit hash
- `--since` - Commits since date (supports shorthand like "1 week ago")
- `--until` - Commits until date (supports shorthand like "1 week ago")
- `--path` - File system path to the git repository
- `--all` - Search all branches for commits by the specified author
- `--interactive` - Run in interactive mode with commit selection UI

**Features:**
- Interactive mode for manual commit selection
- Non-interactive mode for automated cherry-picking
- Progress display with table showing commit hash, date, message, and status
- Statistics on commits applied

#### `git list-changes`

**File:** `ListChangesCommand.cs`

Generates a list of file changes filtered by author and commit range. Returns only file names, not the actual diffs.

**Options:**
- `--author` (required) - Author name or email to filter commits by
- `--start` - Starting commit hash
- `--end` - Ending commit hash
- `--since` - Commits since date
- `--until` - Commits until date
- `--path` - File system path to the git repository
- `--all` - Search all branches
- `--output` - Display format: `fileName` (default), `brief`, or `full`

**Output Modes:**
- `fileName` - Lists only changed file paths
- `brief` - Shows commit hash, branch, and date in a table
- `full` - Tree view with commit details and changed files

**Statistics:**
- Displays distinct file change count
- Displays total change count

#### `git list-prs`

**File:** `ListPullRequestsCommand.cs`

Lists pull requests across all Go projects within a directory.

**Options:**
- `--path` - File system path to search for Go projects

**Features:**
- Automatically discovers Go projects in subdirectories
- Displays PR details: project name, title, state, age, and URL
- Sorted by project name

#### `git auto-pr`

**File:** `AutomatedPullRequestCommand.cs`

Creates a pull request for the current branch, automatically merges it, and monitors the associated GitHub Action progress.

**Options:**
- `--target` - Target branch for the pull request (defaults to project's default branch)
- `--path` - File system path to the git repository

**Workflow:**
1. Creates pull request from current branch to target branch
2. Auto-merges the pull request
3. Monitors GitHub Action deployment status
4. Displays progress for each step

### Deploy Subcommand (`deploy`)

Commands for deployment and synchronization operations.

#### `deploy go-sync`

**File:** `GoRemoteSyncCommand.cs`

Monitors local file system changes and synchronizes them with a remote UAT instance in real-time.

**Options:**
- `--batch` - Number of file changes to batch before uploading (default: 2)
- `--debounce` - Time in seconds to wait before processing a batch (default: 3)
- `--path` - File system path to the Go project

**Features:**
- Real-time file system monitoring
- Batched uploads for efficiency
- Debouncing to prevent excessive syncing
- Live table displaying sync activity

### Logs Subcommand (`logs`)

Commands for retrieving and managing logs.

#### `logs remote`

**File:** `GetRemoteLogCommand.cs`

Retrieves remote logs from Laravel Go applications via SSH.

**Options:**
- `--path` - File system path to the Go project

**Requirements:**
- Must have SSH host configured in `~/.ssh/config`
- Project must be a valid Go Laravel Project with remote log configuration

#### `logs loki`

**File:** `WriteLokiLogCommand.cs`

Pushes log messages to Grafana Cloud Loki.

**Options:**
- `--message` (required) - Log message to send
- `--tags` - Comma-delimited list of key=value pairs for log tags

**Example:**
```bash
go logs loki --message "Deployment completed" --tags "environment=production,service=api"
```

### Config Subcommand (`config`)

Configuration management commands.

#### `config ssh-config-list`

**File:** `SshConfigCommand.cs`

Lists all configured SSH hosts from `~/.ssh/config`.

**Options:**
- `--path` - Optional path to a specific SSH config file

**Display:**
- Shows host name and hostname in a formatted panel

### User Subcommand (`user`)

User management and authentication commands.

#### `user unified-login-signup`

**File:** `UnifiedLoginSignupCommand.cs`

Sends a unified login invitation to a specified email address.

**Options:**
- `--email` (required) - User email address
- `--applicationName` - Application name (default: "go")

**Features:**
- Retrieves access token from RedTeam Platform
- Sends signup invitation via platform service

## Common Parameters

The following parameters are available across multiple commands:

- `--author` - Author name or email to filter commits by
- `--start` - Starting commit hash
- `--end` - Ending commit hash
- `--since` - Commits since date (supports natural language like "1 week ago")
- `--until` - Commits until date
- `--path` - File system path to the repository or project
- `--all` - Search all branches
- `--interactive` - Enable interactive mode
- `--logLevel` - Logging verbosity level

## Architecture

All commands implement the `ICommand<TParameters>` interface and use:
- Dependency injection for services
- Spectre.Console for rich terminal UI
- CommandContext for execution context
- CancellationToken support for graceful cancellation

## Command Metadata

Command metadata is centralized in `CommandMetadata.cs`, which defines:
- Subcommand names and descriptions
- Command names and descriptions
- Common parameter definitions

This metadata is used for:
- Help text generation
- Command registration
- Documentation consistency


# Build and Publish

If you want to build and publish the RedTeamGoCli NuGet package, you can do  the following:

```
1.  Update the package version in the .csproj file if necessary.
2.  dotnet build --configuration Release
3.  dotnet nuget push -s GitHub .\nupkg\rtgo.1.0.3.nupkg - make sure to change package number
```

# Configuration 

RedTeamGoCli uses a configuration file located at `~/USERPROFILE/.secrets/rtgo.json`. 
Before you run this tool, ask a Go team member to provide you with this file.