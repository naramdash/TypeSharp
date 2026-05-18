# Task: NetFx Application Model Compatibility Contract

Status: Done
Queue: Q1
Start Time: 2026-05-18 23:21:45 +09:00
End Time: 2026-05-18 23:24:28 +09:00

## Objective

`docs/goal.md`의 .NET Framework ASP.NET/WCF/worker 호환성 목표를 requirements, feature map, interop contract, feasibility, checklist, traceability로 연결해 장기 작업자가 구현/검증 항목으로 추적할 수 있게 한다.

## Source Of Truth

- [../goal.md](../goal.md)
- [../requirements.md](../requirements.md)
- [../feature-map.md](../feature-map.md)
- [../csharp-interop.md](../csharp-interop.md)
- [../feasibility.md](../feasibility.md)
- [../checklist.md](../checklist.md)
- [../traceability.md](../traceability.md)
- [../../agent.md](../../agent.md)

## Scope

In:
- platform and interop requirements for .NET Framework ASP.NET, WCF, Windows Service, and worker-style hosts
- feature-map classification for application model compatibility
- C# interop contract notes for generated assembly/runtime deployment into existing hosts
- feasibility boundary between MVP assembly compatibility and later templates/smoke suites
- checklist and traceability updates
- agent operating questions and current priority updates

Out:
- ASP.NET project template implementation
- WCF service/client generation
- IIS or Windows Service test harness implementation
- NuGet restore or packaging automation

## Acceptance Criteria

- [x] requirements include ASP.NET/WCF/worker host compatibility expectations.
- [x] feature map classifies .NET Framework application model compatibility.
- [x] csharp interop docs describe generated assembly/runtime shape constraints for existing hosts.
- [x] feasibility separates MVP compatibility contract from later templates/smokes.
- [x] checklist contains follow-up verification items.
- [x] traceability maps the goal to requirements, feature map, and checklist evidence.
- [x] agent guidance includes the new host compatibility contract.

## Verification

Command:

```text
rg -n "ASP.NET|WCF|worker|Windows Service|IIS|web.config|AppDomain" docs agent.md
git diff --check
```

Expected:
- ASP.NET/WCF/worker compatibility appears in goal, requirements, feature map, csharp interop, feasibility, checklist, traceability, and this task packet.
- No whitespace errors are reported.

Result:
- Pass. `rg -n "ASP.NET|WCF|worker|Windows Service|IIS|web.config|AppDomain" docs agent.md`
- Pass. `git diff --check`

## Handoff

Done:
- Added ASP.NET/WCF/worker platform requirements.
- Added feature-map entry 16 for .NET Framework application model compatibility.
- Added C# interop contract notes for existing host deployment and runtime shape.
- Added feasibility boundary for MVP library compatibility versus later host-specific templates/smokes.
- Added checklist follow-up smoke items for ASP.NET, WCF, and worker hosts.
- Updated traceability and agent guidance.

Remaining:
- Implement future ASP.NET/WCF/worker host smoke fixtures once project templates and host integration are in scope.

Blocked:
- None.
