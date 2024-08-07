# Command Line Interface for ConfigCat

[![ConfigCat CLI CI](https://github.com/configcat/cli/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/configcat/cli/actions/workflows/ci.yml)

The ConfigCat Command Line Interface allows you to interact with the <a target="_blank" href="https://configcat.com/docs/advanced/public-api">Public Management API</a> via the command line. It supports most functionality found on the ConfigCat Dashboard. You can manage ConfigCat resources like Feature Flags, Targeting / Percentage rules, Products, Configs, Environments, and more.

<img src="assets/teaser.gif" alt="ConfigCat CLI Feature Flag Create"/>

See the <a target="_blank" href="https://configcat.github.io/cli/">command reference documentation</a> for more information about each available command.

## Getting Started
The following instructions will guide you through the first steps to start using this tool.

### Installation
You can install the CLI on multiple operating systems using the following methods.

<details>
  <summary><strong>Homebrew (macOS / Linux)</strong></summary>

Install the CLI with [Homebrew](https://brew.sh) from [ConfigCat's tap](https://github.com/configcat/homebrew-tap) by executing the following command:
```bash
brew tap configcat/tap
brew install configcat
```

</details>

<details>
  <summary><strong>Snap (Linux)</strong></summary>

Install the CLI with [Snapcraft](https://snapcraft.io/) by executing the following command:
```bash
sudo snap install configcat
```

</details>

<details>
  <summary><strong>Scoop (Windows)</strong></summary>

Install the CLI with [Scoop](https://scoop.sh) from [ConfigCat's bucket](https://github.com/configcat/scoop-configcat) by executing the following command:
```bash
scoop bucket add configcat https://github.com/configcat/scoop-configcat
scoop install configcat
```

</details>

<details>
  <summary><strong>Chocolatey (Windows)</strong></summary>

Install the CLI with [Chocolatey](https://chocolatey.org/) by executing the following command:
```powershell
choco install configcat
```

</details>

<details>
  <summary><strong>.NET tool / NuGet.org</strong></summary>

The CLI can be installed as a [.NET tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools) via the .NET SDK.
```bash
dotnet tool install -g configcat-cli
```

After installing, you can execute the CLI using the `configcat` command:
```bash
configcat scan "/repository" --print --config-id <CONFIG-ID>
```

</details>

<details>
  <summary><strong>Docker</strong></summary>

The CLI can be executed from a [Docker](https://www.docker.com/) image.
```bash
docker pull configcat/cli
```
An example of how to scan a repository for feature flag & setting references with the docker image.
```bash
docker run --rm \
    --env CONFIGCAT_API_HOST=<API-HOST> \
    --env CONFIGCAT_API_USER=<API-USER> \
    --env CONFIGCAT_API_PASS=<API-PASSWORD> \
    -v /path/to/repository:/repository \
  configcat/cli scan "/repository" --print --config-id <CONFIG-ID>
```

</details>

<details>
  <summary><strong>Install Script</strong></summary>

On Unix platforms, you can install the CLI by executing an install script.
```bash
curl -fsSL "https://raw.githubusercontent.com/configcat/cli/main/scripts/install.sh" | bash
```

By default, the script downloads the OS specific artifact from the latest [GitHub Release](https://github.com/configcat/cli/releases) with `curl` and moves it into the `/usr/local/bin` directory.

It might happen, that you don't have permissions to write into `/usr/local/bin`, then you should execute the install script with `sudo`.

```bash
curl -fsSL "https://raw.githubusercontent.com/configcat/cli/main/scripts/install.sh" | sudo bash
```

The script accepts the following input parameters:

Parameter | Description | Default value
--------- | ----------- | -------------
`-d`, `--dir` | The directory where the CLI should be installed. | `/usr/local/bin`
`-v`, `--version` | The desired version to install. | `latest`
`-a`, `--arch` | The desired architecture to install. | `x64`

Available **architecture** values for Linux: `x64`, `musl-x64`, `musl-arm64`, `arm`, `arm64`.

Available **architecture** values for macOS: `x64`, `arm64`.

**Script usage examples**:

*Custom installation directory*:
```bash
curl -fsSL "https://raw.githubusercontent.com/configcat/cli/main/scripts/install.sh" | bash -s -- -d=/path/to/install
```

*Install a different version*:
```bash
curl -fsSL "https://raw.githubusercontent.com/configcat/cli/main/scripts/install.sh" | bash -s -- -v=1.4.2
```

*Install with custom architecture*:
```bash
curl -fsSL "https://raw.githubusercontent.com/configcat/cli/main/scripts/install.sh" | bash -s -- -a=arm
```

</details>

<details>
  <summary><strong>Standalone executables</strong></summary>

You can download the executables directly from [GitHub Releases](https://github.com/configcat/cli/releases) for your desired platform.

</details>

### Configuration
After a successful installation, the CLI must be configured with your <a target="_blank" href="https://app.configcat.com/my-account/public-api-credentials">ConfigCat Management API credentials</a>.

You can do this by using the `configcat setup` command.

<img src="assets/setup.gif" alt="ConfigCat CLI" />

#### Environment Variables
Besides the configuration command above, the CLI can read your credentials from the following environment variables.

Name | Description |
--------- | ----------- |
`CONFIGCAT_API_HOST` | The Management API host. (default: api.configcat.com) | 
`CONFIGCAT_API_USER` | The Management API basic authentication username. |
`CONFIGCAT_API_PASS` | The Management API basic authentication password. | 

> When any of these environment variables are set, the CLI will use them over the local values set by the `configcat setup` command.

## About ConfigCat
ConfigCat is a feature flag and configuration management service that lets you separate releases from deployments. You can turn your features ON/OFF using <a href="https://app.configcat.com" target="_blank">ConfigCat Dashboard</a> even after they are deployed. ConfigCat lets you target specific groups of users based on region, email or any other custom user attribute.

ConfigCat is a <a href="https://configcat.com" target="_blank">hosted feature flag service</a>. Manage feature toggles across frontend, backend, mobile, desktop apps. <a href="https://configcat.com" target="_blank">Alternative to LaunchDarkly</a>. Management app + feature flag SDKs.

- [Documentation](https://configcat.com/docs/advanced/cli)
- [ConfigCat](https://configcat.com)
- [Blog](https://configcat.com/blog)
