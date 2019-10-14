require 'nokogiri'

configatron.product_name = "PayPalHttp Dotnet"
CSPROJ = "PayPalHttp-Dotnet/PayPalHttp-Dotnet.csproj"

# Custom validations
def test
  CommandProcessor.command("dotnet test PayPalHttp-Dotnet.Tests", live_output=true)
end

def package_version
  File.open(CSPROJ) do |f|
    xml = Nokogiri::XML(f)
    return xml.at_xpath('//Version').content
  end
end

def validate_version_match
	if package_version != @current_release.version
		Printer.fail("package version #{package_version} does not match changelog version #{@current_release.version}.")
		abort()
	end

	Printer.success("package version #{package_version} matches latest changelog version #{@current_release.version}.")
end

def validate_present(tool, install_command)
  tool_path = `which #{tool}`
  if tool_path.rstrip == ""
    Printer.fail("#{tool} not installed - please run `#{install_command}`")
    abort()
  else
    Printer.success("#{tool} found at #{tool_path}")
  end
end

def validate_dotnet
  validate_present("dotnet", "(Install the dotnet runtime https://www.microsoft.com/net/core)")
end

configatron.custom_validation_methods = [
	method(:validate_version_match),
  method(:validate_dotnet),
  method(:test),
]

def clean
  CommandProcessor.command("dotnet clean")
end

def build_method
  CommandProcessor.command("dotnet pack -c Release PayPalHttp-Dotnet")
end

configatron.build_method = method(:build_method)

def update_version_method(version, semver_type)
  contents = File.read(CSPROJ)
  xml = Nokogiri::XML(contents)
  xml.at_xpath('//Version').content = version
  xml.at_xpath('//PackageReleaseNotes').content = @current_release.changelog

  File.open(CSPROJ, 'w') do |f|
    f << xml.to_html
  end
end

configatron.update_version_method = method(:update_version_method)

def publish_to_package_manager(version)
  nupkg = "PayPalHttp-Dotnet/bin/Release/PayPalHttp-Dotnet.#{version}.nupkg"
  CommandProcessor.command("nuget push #{nupkg} -Source https://api.nuget.org/v3/index.json")
end

def wait_for_package_manager(version)
  # noop
end

configatron.publish_to_package_manager_method = method(:publish_to_package_manager)
configatron.wait_for_package_manager_method = method(:wait_for_package_manager)

# Miscellania
configatron.release_to_github = true
configatron.prerelease_checklist_items []
