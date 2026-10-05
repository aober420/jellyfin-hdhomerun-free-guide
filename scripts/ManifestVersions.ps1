function Merge-PluginVersions {
    param(
        [Parameter(Mandatory)] $NewVersion,
        [AllowEmptyCollection()] [object[]] $ExistingVersions = @()
    )

    $olderVersions = @($ExistingVersions | Where-Object { $_ -and $_.version -ne $NewVersion.version })
    @($NewVersion) + $olderVersions | Sort-Object { [version]$_.version } -Descending
}
