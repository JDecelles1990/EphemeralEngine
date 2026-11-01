#pwsh ./scripts/Test-MarkdownLinks.ps1 -DocsRoot "docs"

[CmdletBinding()]
param(
    [string]$DocsRoot = "docs"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Test-Path -Path $DocsRoot)) {
    Write-Error "Target documentation directory not found: $DocsRoot"
    exit 1
}

function ConvertTo-Slug {
    param([string]$Heading)
    # Remove leading hashes and trim
    $text = $Heading -replace '^#+\s*', ''
    # Remove HTML tags and markdown formatting (bold, italics, inline code)
    $text = $text -replace '<[^>]+>', '' -replace '[`*_~]', ''
    # Convert to lowercase
    $text = $text.ToLowerInvariant().Trim()
    # Replace non-alphanumeric chars (except hyphens and spaces) with empty
    $text = $text -replace '[^a-z0-9\s-]', ''
    # Replace whitespace and repeated hyphens with a single hyphen
    $text = $text -replace '\s+', '-' -replace '-+', '-'
    return $text.Trim('-')
}

$mdFiles = Get-ChildItem -Path $DocsRoot -Recurse -Filter "*.md"
$linkRegex = '(?<!\!)\[.*?\]\((?!https?:\/\/|mailto:)(.*?)\)'
$headingRegex = '^#{1,6}\s+(.+)$'

# Pre-index headings for all Markdown files
$fileHeadings = @{}
foreach ($file in $mdFiles) {
    $normalizedPath = [System.IO.Path]::GetFullPath($file.FullName)
    $lines = Get-Content -Path $file.FullName
    $slugs = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($line in $lines) {
        if ($line -match $headingRegex) {
            $slug = ConvertTo-Slug -Heading $line
            if (-not [string]::IsNullOrWhiteSpace($slug)) {
                $null = $slugs.Add($slug)
            }
        }
    }
    $fileHeadings[$normalizedPath] = $slugs
}

$brokenLinks = [System.Collections.Generic.List[PSCustomObject]]::new()
$totalLinksChecked = 0

foreach ($file in $mdFiles) {
    $currentDir = $file.DirectoryName
    $content = Get-Content -Path $file.FullName -Raw

    $matches = [regex]::Matches($content, $linkRegex)
    foreach ($m in $matches) {
        $rawTarget = $m.Groups[1].Value.Trim()
        if ([string]::IsNullOrWhiteSpace($rawTarget) -or $rawTarget.StartsWith('#') -and $rawTarget.Length -eq 1) {
            continue
        }

        $totalLinksChecked++
        $targetFilePart = ""
        $anchorPart = ""

        if ($rawTarget -match '^(?<file>[^#]*)(?:#(?<anchor>.*))?$') {
            $targetFilePart = $Matches['file']
            $anchorPart = $Matches['anchor']
        }

        # Resolve target full path
        $targetFullPath = if ([string]::IsNullOrWhiteSpace($targetFilePart)) {
            $file.FullName
        }
        else {
            [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($currentDir, $targetFilePart))
        }

        # 1. Validate File Existence
        if (-not (Test-Path -Path $targetFullPath -PathType Leaf)) {
            $brokenLinks.Add([PSCustomObject]@{
                    SourceFile = $file.FullName.Replace($PWD.Path, "").TrimStart('\', '/')
                    TargetLink = $rawTarget
                    Reason     = "Target file does not exist ($targetFilePart)"
                })
            continue
        }

        # 2. Validate Anchor (if present and target is Markdown)
        if (-not [string]::IsNullOrWhiteSpace($anchorPart) -and $targetFullPath.EndsWith('.md', [System.StringComparison]::OrdinalIgnoreCase)) {
            if ($fileHeadings.ContainsKey($targetFullPath)) {
                $expectedSlug = $anchorPart.ToLowerInvariant().Trim()
                if (-not $fileHeadings[$targetFullPath].Contains($expectedSlug)) {
                    $brokenLinks.Add([PSCustomObject]@{
                            SourceFile = $file.FullName.Replace($PWD.Path, "").TrimStart('\', '/')
                            TargetLink = $rawTarget
                            Reason     = "Anchor '#$expectedSlug' not found in target file"
                        })
                }
            }
        }
    }
}

Write-Host "`nDocumentation Link Validation Report" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Files Scanned : $($mdFiles.Count)"
Write-Host "Links Checked : $totalLinksChecked"

if ($brokenLinks.Count -gt 0) {
    Write-Host "Broken Links  : $($brokenLinks.Count)`n" -ForegroundColor Red
    $brokenLinks | Format-Table -AutoSize | Out-String | Write-Host -ForegroundColor Yellow
    exit 1
}

Write-Host "All internal links and anchors are valid!`n" -ForegroundColor Green
exit 0
