\# Session 001: Project Scaffolding (T0.01)



\*\*Date:\*\* 2025-10-10

\*\*Task ID:\*\* T0.01

\*\*Phase:\*\* Phase 0 - Foundation



---



\## Context for Claude (Senior Developer)



You are the senior developer on the NetSuite RAG project. I am the developer who will execute your instructions step-by-step.



\### Project Information

\- \*\*Project:\*\* NetSuite RAG Reporting Tool

\- \*\*Location:\*\* D:\\Development\\NetsuiteRAG

\- \*\*Tech Stack:\*\* .NET 9, C# 13, Angular 20, PostgreSQL, Redis, Qdrant, Ollama

\- \*\*My Role:\*\* Execute your instructions, provide feedback, report results



\### Reference Documents (I will attach these)

1\. PROJECT\_MASTER.md - Current project status

2\. NetSuite\_RAG\_Reporting\_Tool\_PRD\_v3.1.md - Requirements

3\. NetSuite\_RAG\_System\_Architecture\_v3.1.md - Architecture

4\. NetSuite\_RAG\_Tasks\_and\_User\_Stories\_v3.1.md - Task breakdown

5\. Tech\_Stack\_Best\_Practices\_No\_DSPy\_v2.md - Tech guidelines



\### My Environment (Verified ✓)

\- Windows 11

\- PowerShell at: D:\\Development\\NetsuiteRAG

\- PostgreSQL: localhost:5432 ✓

\- Redis: localhost:6379 ✓

\- Qdrant: localhost:6333 ✓

\- Ollama: localhost:11434 ✓

\- .NET SDK: 9.0.305 ✓

\- Node.js: v22.18.0 ✓

\- Git initialized ✓



\### Previous Session

This is the FIRST development session. Project structure created, all services verified running.



---



\## Task for This Session



\*\*Task ID:\*\* T0.01

\*\*Task Name:\*\* Feature Flag Service bootstrap + Project scaffolding

\*\*Description:\*\* Create the .NET solution structure with all projects and Aspire orchestration



\### From Task Document (T0.01)

\*\*Inputs:\*\* Repository, environments

\*\*Work:\*\* 

\- Create .NET solution with multiple projects

\- Set up Aspire AppHost for orchestration

\- Create project structure (API, Shared, UI, Tests)

\- Configure project references

\- Ensure solution builds



\*\*Outputs:\*\* 

\- Complete solution structure

\- AppHost configured

\- All projects created and referenced

\- Can run: dotnet build



\*\*Acceptance:\*\*

\- Solution file exists

\- All projects created (AppHost, Api, Shared, Tests)

\- Projects reference each other correctly

\- Solution builds without errors

\- Aspire orchestration configured



\### Success Criteria

1\. ✓ .NET solution created

2\. ✓ Aspire AppHost project exists

3\. ✓ API project created

4\. ✓ Shared library created

5\. ✓ Test project created

6\. ✓ Angular project created

7\. ✓ All project references configured

8\. ✓ `dotnet build` succeeds

9\. ✓ Can run `dotnet run --project src/NetSuiteRAG.AppHost`



---



\## Your Instructions, Claude



Please provide step-by-step instructions to create the complete solution structure.



\### Important Rules:

\- Give me ONE step at a time

\- Wait for my "Step X done" confirmation before proceeding

\- Provide EXACT PowerShell commands

\- Tell me what output to expect

\- Explain what each command does



\### What I Need From You:

1\. \*\*Step-by-step commands\*\* to create the solution and all projects

2\. \*\*Explanation\*\* of what each command does

3\. \*\*Expected output\*\* so I know it worked

4\. \*\*Verification commands\*\* to confirm success



---



\## My Commitment



After each step you provide:

\- I will execute the command exactly

\- I will report back:

&nbsp; - ✅ "Step X done" + paste the output

&nbsp; - ❌ Complete error message if it fails

&nbsp; - ❓ Questions if anything is unclear



I will NOT move to the next step until you tell me to.



---



\## Session Progress



\[I will track progress here as we work]



---



Let's begin! What's Step 1, Claude?

