# save as repush-tag.ps1
param (
    [Parameter(Mandatory = $true)]
    [string]$TagName,

    [Parameter(Mandatory = $true, ValueFromRemainingArguments = $true)]
    [string[]]$TagMessageParts
)

# Join the remaining arguments into a single string for the tag message
$TagMessage = $TagMessageParts -join ' '

# Delete local tag
git tag -d $TagName

# Create annotated tag with message
git tag -a $TagName -m "$TagMessage"

# Force push tag to remote
git push -f origin $TagName