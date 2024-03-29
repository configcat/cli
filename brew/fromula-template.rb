class Configcat < Formula
  desc "The ConfigCat Command Line Interface allows you to interact with the ConfigCat Management API via the command line. It supports most functionality found on the ConfigCat Dashboard. You can manage ConfigCat resources like Feature Flags, Targeting / Percentage rules, Products, Configs, Environments, and more."
  homepage "https://configcat.com"
  version "#VERSION_PLACEHOLDER#"
  license "MIT"
  
  if OS.mac? && Hardware::CPU.intel?
    url "#OSX-TAR-PATH#"
    sha256 "#OSX-TAR-SUM#"
  end
  if OS.mac? && Hardware::CPU.arm?
    url "#OSX-ARM-TAR-PATH#"
    sha256 "#OSX-ARM-TAR-SUM#"
  end
  if OS.linux? && Hardware::CPU.intel?
    url "#LINUX-TAR-PATH#"
    sha256 "#LINUX-TAR-SUM#"
  end
  
  def install
    bin.install "configcat"
  end

  test do
    assert_match "#VERSION_PLACEHOLDER#", shell_output("#{bin}/configcat --version")
  end
end
