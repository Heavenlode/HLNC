# Script to copy and rename markdown files from Content folder to root directory
$scriptDir = $PSScriptRoot
$projectDir = Split-Path -Parent $scriptDir  # Go up one level from script directory
$contentDir = Join-Path -Path $projectDir -ChildPath "Content"
$rootDir = $projectDir

# Output debugging information
Write-Host "Script directory: $scriptDir"
Write-Host "Project directory: $projectDir"
Write-Host "Content directory: $contentDir"
Write-Host "Root directory: $rootDir"

# Check if Content directory exists
if (-Not (Test-Path -Path $contentDir)) {
    Write-Host "ERROR: Content directory not found at $contentDir" -ForegroundColor Red
    exit 1
}

# First, clean up old markdown files from the root directory (non-recursively)
Write-Host "Cleaning up old markdown files from $rootDir..."
$oldMdFiles = Get-ChildItem -Path $rootDir -Filter "*.md" -File -ErrorAction SilentlyContinue | 
              Where-Object { $_.DirectoryName -eq $rootDir } # Ensure we're only getting files directly in root

# Count how many files we found
$oldFileCount = ($oldMdFiles | Measure-Object).Count
Write-Host "Found $oldFileCount markdown files to clean up."

# Remove each old file, but be careful not to delete important files
foreach ($oldFile in $oldMdFiles) {
    # Skip README.md or other important files you want to keep
    if ($oldFile.Name -eq "README.md" -or $oldFile.Name -eq "LICENSE.md") {
        Write-Host "Skipping $($oldFile.Name) as it's a protected file."
        continue
    }
    
    Remove-Item -Path $oldFile.FullName -Force
}

# Function to process file name
function Get-FormattedFileName {
    param (
        [string]$filePath,
        [string]$baseDir
    )
    
    try {
        # Get the original filename
        $originalFileName = [System.IO.Path]::GetFileName($filePath)
        
        # If the file starts with underscore, preserve that in the output
        $startsWithUnderscore = $originalFileName.StartsWith("_")
        
        # Get directories inside Content folder, not including Content itself
        $fullPath = [System.IO.Path]::GetFullPath($filePath)
        
        # Get the file's directory (excluding the file name)
        $fileDir = [System.IO.Path]::GetDirectoryName($fullPath)
        
        # Skip the Content folder by making sure we're getting subdirectories
        # inside Content, not Content itself
        if ($fileDir -eq $baseDir) {
            # File is directly in Content folder, no subdirectories
            $subdirPath = ""
        } else {
            # File is in a subdirectory of Content
            $subdirPath = $fileDir.Substring($baseDir.Length).TrimStart([System.IO.Path]::DirectorySeparatorChar)
        }
        
        # Get the filename WITHOUT extension
        $fileNameWithoutExt = [System.IO.Path]::GetFileNameWithoutExtension($fullPath)
        
        # Get the extension (including the dot)
        $extension = [System.IO.Path]::GetExtension($fullPath)
        
        # Replace spaces and periods with underscores in filename (but not the extension)
        $fileNameWithoutExt = $fileNameWithoutExt -replace '[\s\.]', '_'
        
        # If there are subdirectories, format them and include in the final name
        if (![string]::IsNullOrEmpty($subdirPath)) {
            # Split subdirectory path into parts
            $dirParts = $subdirPath.Split([System.IO.Path]::DirectorySeparatorChar)
            
            # Clean up each directory name
            $dirParts = $dirParts | ForEach-Object { $_ -replace '[\s\.]', '_' }
            
            # Join them with underscores and combine with filename
            $formattedName = ($dirParts -join "_") + "_" + $fileNameWithoutExt + $extension
        } else {
            # No subdirectories, just use the cleaned filename
            $formattedName = $fileNameWithoutExt + $extension
        }
        
        # If the original file started with underscore, ensure it's preserved
        if ($startsWithUnderscore -and -not $formattedName.StartsWith("_")) {
            $formattedName = "_" + $formattedName
        }
        
        return $formattedName
    }
    catch {
        Write-Host "ERROR processing file $filePath : $_" -ForegroundColor Red
        # Return original filename as fallback
        return [System.IO.Path]::GetFileName($filePath)
    }
}

# Function to extract a better title from the filename
function Get-HumanReadableTitle {
    param (
        [string]$fileName
    )
    
    # Remove the extension
    $nameWithoutExt = [System.IO.Path]::GetFileNameWithoutExtension($fileName)
    
    # Check if the filename has a number prefix like "1. " or "1_" or "1."
    if ($nameWithoutExt -match '^(\d+)[_\.\s]+(.+)$') {
        # Return format like "1. Title"
        return "$($Matches[1]). $($Matches[2] -replace '_', ' ')"
    } elseif ($nameWithoutExt -match '^(\d+)(.+)$') {
        # Handle cases like "1Introduction"
        return "$($Matches[1]). $($Matches[2] -replace '_', ' ')"
    } else {
        # For other cases, just replace underscores with spaces
        return $nameWithoutExt -replace '_', ' '
    }
}

# Get all markdown files in Content directory and its subdirectories
Write-Host "Searching for markdown files in $contentDir..."
$mdFiles = Get-ChildItem -Path $contentDir -Filter "*.md" -Recurse -ErrorAction SilentlyContinue

if ($mdFiles.Count -eq 0) {
    Write-Host "WARNING: No markdown files found in $contentDir" -ForegroundColor Yellow
} else {
    Write-Host "Found $($mdFiles.Count) markdown files to process."
}

# Create a list to store the processed filenames and their original names
$processedFiles = @()

# Track successfully copied files
$copiedCount = 0

foreach ($file in $mdFiles) {
    $newFileName = Get-FormattedFileName -filePath $file.FullName -baseDir $contentDir
    $destination = Join-Path -Path $rootDir -ChildPath $newFileName
    
    try {
        Copy-Item -Path $file.FullName -Destination $destination -Force
        $copiedCount++
        
        # Extract the parent directory name
        $fileDir = Split-Path -Parent $file.FullName
        $parentDir = Split-Path -Leaf $fileDir
        if ($parentDir -eq "Content") {
            $parentDir = $null # File is directly in Content folder
        }
        
        # Extract file information for sitemap
        $originalNameWithoutExt = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
        $humanTitle = Get-HumanReadableTitle -fileName $file.Name
        
        # Add this file to our processed files list
        $fileInfo = [PSCustomObject]@{
            OriginalName = $file.Name
            OriginalPath = $file.FullName
            OriginalNameWithoutExt = $originalNameWithoutExt
            NewName = $newFileName
            Title = $humanTitle
            ParentDir = $parentDir
            IsSubfile = $false # Will be set to true for child files later
            Url = $newFileName
        }
        $processedFiles += $fileInfo
    }
    catch {
        Write-Host "ERROR copying file $($file.FullName): $_" -ForegroundColor Red
    }
}

# Now determine parent-child relationships
# First, identify all potential parent files (files directly in Content folder)
$parentFiles = $processedFiles | Where-Object { $_.ParentDir -eq $null }

# Then process each file that's in a subdirectory
foreach ($file in $processedFiles) {
    # Skip files that are directly in the Content folder
    if ($null -eq $file.ParentDir) {
        continue
    }

    # Look for a potential parent file that corresponds to the directory name
    $potentialParentName = $file.ParentDir
    
    # First try to find a parent in the root Content folder
    $parentFile = $parentFiles | Where-Object {
        $_.OriginalNameWithoutExt -eq $potentialParentName -or 
        $_.OriginalNameWithoutExt -match [regex]::Escape($potentialParentName)
    } | Select-Object -First 1
    
    # If no parent found in root, look for a parent in any subdirectory
    if ($null -eq $parentFile) {
        $parentFile = $processedFiles | Where-Object {
            $_.OriginalNameWithoutExt -eq $potentialParentName -or 
            $_.OriginalNameWithoutExt -match [regex]::Escape($potentialParentName)
        } | Select-Object -First 1
    }
    
    if ($null -ne $parentFile) {
        # This file is a child of a parent file
        $file.IsSubfile = $true
    }
}

# Generate Documentation.sitemap file
Write-Host "Generating Documentation.sitemap file..."
$sitemapPath = Join-Path -Path $rootDir -ChildPath "Documentation.sitemap"

try {
    # Find the root-level parent nodes
    $parentNodes = $processedFiles | Where-Object { $_.ParentDir -eq $null } | Sort-Object -Property NewName
    
    # Create XML content as a string
    $xmlContent = @"
<?xml version="1.0" encoding="utf-8"?>
<siteMap xmlns="http://schemas.microsoft.com/AspNet/SiteMap-File-1.0">
"@

    # Process each potential parent node
    $isFirstParent = $true
    foreach ($parent in $parentNodes) {
        # Find all child files of this parent
        $childFiles = $processedFiles | Where-Object { 
            # Get the clean name of the parent (without number prefix)
            $cleanParentName = $parent.OriginalNameWithoutExt -replace '^\d+\.\s*', ''
            
            # Check if this file is a child
            $isChild = $_.IsSubfile -eq $true -and (
                # Direct children in the parent's directory
                $_.ParentDir -eq $cleanParentName
            )

            $isChild
        } | Sort-Object -Property NewName
        
        # If this node has children, make it a parent node
        $defaultAttributes = if ($isFirstParent) { ' isDefault="true" isSelected="true"' } else { '' }
        
        # Start the parent node
        $xmlContent += "`n  <siteMapNode title=`"$($parent.Title)`" url=`"$($parent.Url)`"$defaultAttributes>"
        
        # Add child nodes
        foreach ($child in $childFiles) {
            
            # Find nested children for this child
            $nestedChildren = $processedFiles | Where-Object {
                $cleanChildName = $child.OriginalNameWithoutExt -replace '^\d+\.\s*', ''
                
                $isNestedChild = $_.IsSubfile -eq $true -and
                    $_.ParentDir -eq $cleanChildName

                $isNestedChild
            } | Sort-Object -Property NewName
            
            
            # Always create a parent node for the child, even if it has no children
            $xmlContent += "`n    <siteMapNode title=`"$($child.Title)`" url=`"$($child.Url)`">"
            
            # Add nested children if any exist
            foreach ($nestedChild in $nestedChildren) {
                $xmlContent += "`n      <siteMapNode title=`"$($nestedChild.Title)`" url=`"$($nestedChild.Url)`" />"
            }
            
            # Close the child node
            $xmlContent += "`n    </siteMapNode>"
        }
        
        # Close the parent node
        $xmlContent += "`n  </siteMapNode>"
        
        $isFirstParent = $false
    }

    # Close the XML
    $xmlContent += "`n</siteMap>"

    # Write the XML content to the file
    $xmlContent | Out-File -FilePath $sitemapPath -Encoding utf8

    Write-Host "Documentation.sitemap created successfully." -ForegroundColor Green
}
catch {
    Write-Host "ERROR generating Documentation.sitemap: $_" -ForegroundColor Red
}

Write-Host "Process completed: $copiedCount files copied, $oldFileCount old files removed." -ForegroundColor Green