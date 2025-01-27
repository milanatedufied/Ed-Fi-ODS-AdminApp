# Getting Started

To apply these build configuration settings on a new TeamCity environment (min
version: `2019.2.2`):

1. Create a "root" project that will contain this work as a sub-project, e.g.
   "ODS Tools"
1. Ensure these parameters are set:

    ```none
    github.organization = <org name or username>
    github.username = <username>
    github.accessToken.protected (password type) = <access token>
    github.accessToken = %github.accessToken.protected%
    git.branch.default = main
    git.branch.specification = +:refs/heads/(*)
                               +:(refs/pull/*/head)

    # Only necessary for the Admin App Installer project
    octopus.deployTimeout = 00:30:00  (or lower number of minutes if you prefer)
    octopus.server = <base URL for your Octopus Deploy server>

    # Used by both the Web and Installer projects when publishing or deploying    
    azureArtifacts.eedFiBuildAgent.accessToken = <get access token from the Azure Artifacts>
    azureArtifacts.edFiBuildAgent.userName = <the actual user name in Azure Artifacts>
    azureArtifacts.feed.nuget = <NuGet feed URL>

    ```

    If pull from an organization, use that organization's name. Else use own
    GitHub user name to pull from your fork.

1. Create a VCS Root:
    * Type: `Git`
    * Name: `ODS Admin App`
    * Fetch Url: `https://github.com/%github.organization%/Ed-Fi-ODS-AdminApp`
    * Default Branch: `%git.branch.default%`
    * Branch Specification: `%git.branch.specification%`
    * Authentication method: `anonymous`
1. Create a sub-project named "ODS AdminApp - Kotlin"
1. Turn on Versioned Settings:
    * Synchronization enabled: `true`
    * Project settings VCS root: `ODS Admin App`
    * When build starts: `use settings from VCS`
    * Store secure values outside of VCS: `true`
    * Settings format: `kotlin`
    * Generate portable DSL scripts: `true`
1. Click Apply.
1. If prompted to choose either committing current changes or reading from the
   VCS root, then choose to read from the VCS root.
1. Else click on the "Load project settings from VCS..." button.
