# GitHub Actions Workflows

## Build, Test, and Package

This workflow automatically builds, tests, and publishes the Promty NuGet package.

### How to Release a New Version

1. **Update your code** and commit your changes

2. **Create and push a version tag**:
   ```bash
   # Create a tag (e.g., v1.0.0, v1.2.3, v2.0.0-beta.1)
   git tag v1.0.0

   # Push the tag to GitHub
   git push origin v1.0.0
   ```

3. **The workflow will automatically**:
   - Build the project
   - Run all tests
   - Create a NuGet package with the version from the tag
   - Upload the package as a GitHub artifact
   - Publish to NuGet.org (if configured)

### Version Naming Convention

Follow [Semantic Versioning](https://semver.org/):
- `v1.0.0` - Major release
- `v1.1.0` - Minor release (new features, backwards compatible)
- `v1.0.1` - Patch release (bug fixes)
- `v2.0.0-beta.1` - Pre-release version

### Setting Up NuGet.org Publishing

To enable automatic publishing to NuGet.org:

1. **Get a NuGet API Key**:
   - Go to https://www.nuget.org/
   - Sign in and go to your account settings
   - Create an API key with "Push" permissions

2. **Add the API key to GitHub Secrets**:
   - Go to your GitHub repository
   - Navigate to Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `NUGET_API_KEY`
   - Value: Your NuGet API key

3. **Push a version tag** and the workflow will automatically publish

### Manual Trigger

You can also manually trigger the workflow:
1. Go to Actions tab in GitHub
2. Select "Build, Test, and Package" workflow
3. Click "Run workflow"
4. It will use the latest tag version

### Viewing Package Artifacts

After each run:
- Go to the workflow run in the Actions tab
- Scroll to the "Artifacts" section at the bottom
- Download the `nuget-package` artifact to get the .nupkg file

### Example Release Process

```bash
# 1. Make your changes
git add .
git commit -m "Add new feature"

# 2. Create and push a version tag
git tag v1.1.0
git push origin main
git push origin v1.1.0

# 3. Wait for GitHub Actions to complete
# 4. Your package is now published!
```

### Troubleshooting

**Tests are failing:**
- Run `dotnet test` locally first to ensure all tests pass
- Fix any failing tests before tagging

**Version already exists on NuGet:**
- You cannot republish the same version
- Create a new tag with an incremented version number

**Build is failing:**
- Check the Actions tab for detailed error logs
- Ensure the tag format is correct (v1.2.3)
