# Dependency exceptions

The `OSPO compliance` workflow fails the build when `dotnet list package --vulnerable
--include-transitive` reports any advisory. This file is the only supported way to accept a
finding temporarily, and every entry must be time-bound and owned.

An exception is acceptable only when **all** of the following hold:

1. No fixed version is available, or the fixed version is not yet reachable from the feeds
   used by this project.
2. The vulnerable code path is not reachable in this project, or a mitigation is in place.
3. An owner and an expiry date are recorded below.
4. A tracking issue exists.

Expired entries must be removed or renewed. An expired entry is treated as a build failure,
not as an accepted risk.

## Active exceptions

| Package | Version | Advisory | Reachable here? | Mitigation | Owner | Expires | Issue |
| --- | --- | --- | --- | --- | --- | --- | --- |
| _none_ | | | | | | | |

## Retired exceptions

| Package | Advisory | Resolution | Date |
| --- | --- | --- | --- |
| _none_ | | | |