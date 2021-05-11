# Command Line Interface for ConfigCat (beta)

[![ConfigCat CLI CI](https://github.com/configcat/cli/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/configcat/cli/actions/workflows/ci.yml)

The ConfigCat Command Line Interface allows you to interact with the ConfigCat Management API. It supports most functionality found on the ConfigCat Dashboard. You can manage ConfigCat resources like Feature Flags, Targeting / Percentage rules, Products, Configs, Environments, and more.

<img src="assets/teaser.gif" alt="ConfigCat CLI Feature Flag Create"/>

## About ConfigCat
ConfigCat is a feature flag and configuration management service that lets you separate releases from deployments. You can turn your features ON/OFF using <a href="https://app.configcat.com" target="_blank">ConfigCat Dashboard</a> even after they are deployed. ConfigCat lets you target specific groups of users based on region, email or any other custom user attribute.

ConfigCat is a <a href="https://configcat.com" target="_blank">hosted feature flag service</a>. Manage feature toggles across frontend, backend, mobile, desktop apps. <a href="https://configcat.com" target="_blank">Alternative to LaunchDarkly</a>. Management app + feature flag SDKs.

## Getting Started
The following instructions will guide you through the first steps to start using this tool.

### Installation
As the development of the CLI is just in beta phase, there is no official package available yet. It'll be distributed through [snapcraft.io](https://snapcraft.io/), [Chocolatey](https://community.chocolatey.org/packages), [Homebrew](https://brew.sh), and [docker](https://www.docker.com/) in the future.

In the meantime, you can download the binaries directly from [GitHub Releases](https://github.com/configcat/cli/releases).

#### via Install Script
You can install the CLI by executing an install script on Unix platforms. 
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
`-a`, `--arch` | The desired architecture to install | `x64`

The possible **architecture** values for Linux: `x64`, `musl-x64`, `arm`, `arm64`.

**Usage examples**:

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

### Setup
After a successful installation, the CLI must be configured with your [ConfigCat Management API credentials](https://app.configcat.com/my-account/public-api-credentials).

<img src="assets/setup.gif" alt="ConfigCat CLI" />

## Useful links
- Documentation (Coming soon)
- [ConfigCat](https://configcat.com)
- [Blog](https://configcat.com/blog)
