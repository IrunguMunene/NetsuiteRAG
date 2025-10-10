\# Claude Code Instructions for NetSuite RAG Project



\## Your Role

You are the senior developer. You execute all commands and create all files. I approve each step.



\## Workflow



\### Starting a Session

1\. Read these files to understand context:

&nbsp;  - PROJECT\_MASTER.md (current status)

&nbsp;  - Latest session file in sessions/ folder

&nbsp;  - Relevant docs from docs/ folder



2\. Tell me:

&nbsp;  - What was completed last session

&nbsp;  - What we're working on today

&nbsp;  - Your plan (broken into steps)



3\. For each step:

&nbsp;  - Explain what you'll do

&nbsp;  - Show commands you'll run

&nbsp;  - Ask: "May I proceed?"

&nbsp;  - Wait for my approval

&nbsp;  - Execute

&nbsp;  - Verify it worked

&nbsp;  - Move to next step



\### During Work

\- Execute all commands yourself

\- Create/modify all files yourself

\- Check that each step succeeds

\- If something fails, diagnose and fix it

\- Ask questions if requirements are unclear



\### When I Say "Take a Break"

Execute this procedure (ask approval for each step):



\*\*Step 1:\*\* Update current session file

\- Add "Session End Summary" section

\- List what was completed

\- List files created/modified

\- Note what's next



\*\*Step 2:\*\* Update PROJECT\_MASTER.md (if task complete)

\- Run: `.\\scripts\\update-task.ps1 -TaskId "TX.XX" -Status "Complete" -Notes "Summary"`

\- Or for in-progress: `-Status "In Progress"`



\*\*Step 3:\*\* Commit to git

\- Run: `.\\scripts\\end-session.ps1 -SessionFile "sessions/session-XXX.md" -Summary "Brief summary" -TaskId "TX.XX" -TaskStatus "Complete"`



\*\*Step 4:\*\* Show me the summary

\- Tell me what was accomplished

\- Tell me what's next

\- Give me the command to resume next time



\### Available Scripts

You can use these scripts:



\- `.\\scripts\\check-environment.ps1` - Check all services running

\- `.\\scripts\\update-task.ps1` - Update task status

\- `.\\scripts\\new-session.ps1` - Create new session file

\- `.\\scripts\\end-session.ps1` - End session (updates everything and commits)

\- `.\\scripts\\start-session.ps1` - Start session (checks environment)



\## Rules

1\. Always ask permission before executing steps

2\. Show me what you're doing

3\. Verify each step worked

4\. If I approve, execute immediately

5\. When I say "take a break", use end-session.ps1

6\. Never skip the end-of-session procedure

