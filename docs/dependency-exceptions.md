# Dependency exceptions

The `OSPO compliance` workflow fails the build when `dotnet list package --vulnerable
--include-transitive` reports any advisory. Entries here document risks for review but do not
bypass the dependency audit; vulnerable packages must be updated for the workflow to pass.

To document an advisory for review, record **all** of the following:

1. No fixed version is available, or the fixed version is not yet reachable from the feeds
   used by this project.
2. The vulnerable code path is not reachable in this project, or a mitigation is in place.
3. An owner and an expiry date are recorded below.
4. A tracking issue exists.

Expired records must be removed or renewed. Recording an advisory never exempts it from CI.

## Open advisory records (not CI exemptions)

| Package | Version | Advisory | Reachable here? | Mitigation | Owner | Expires | Issue |
| --- | --- | --- | --- | --- | --- | --- | --- |
| _none_ | | | | | | | |

## Resolved advisory records

| Package | Advisory | Resolution | Date |
| --- | --- | --- | --- |
| _none_ | | | |