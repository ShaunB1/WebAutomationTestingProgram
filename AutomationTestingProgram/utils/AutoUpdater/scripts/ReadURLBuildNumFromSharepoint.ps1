param (
    [Parameter(Mandatory=$true)]
    [string]$itemName
)

Import-Module SharePointPnPPowerShellOnline

$WarningPreference = 'SilentlyContinue'
# SharePoint Online
$siteUrl = "https://ontariogov.sharepoint.com/sites/CSC-Intranet/DDSB/AA/BuildsInfrast"

# List information
$listName = "Environments"
$nameColumn = "Title"
$buildVersionColumn = "Build_x0020_Version"
$urlColumn = "URL"

# Connect to SharePoint Online using Web Login
Connect-PnPOnline -Url $siteUrl -UseWebLogin

# Get the item by its name
$item = Get-PnPListItem -List $listName -Query "<View><Query><Where><Eq><FieldRef Name='$nameColumn'/><Value Type='Text'>$itemName</Value></Eq></Where></Query></View>"

if ($item -ne $null) {
    # Retrieve the "URL" and "Build Version" column values
    $urlValue = $item[$urlColumn]
    $url = $urlValue.Url
    $buildVersion = $item[$buildVersionColumn]
    
    Write-Host "URL: $url"
    Write-Host "BUILD: $buildVersion"
} else {
    Write-Host "Item '$itemName' not found."
}

# Disconnect from SharePoint Online
Disconnect-PnPOnline