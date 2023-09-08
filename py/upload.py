from argparse import ArgumentParser
from github import Github, Repository, GitRelease

"""
Upload a release. This is called by the Ubuntu 18 Docker container.
"""

parser = ArgumentParser()
parser.add_argument("--token", type=str)
parser.add_argument("--version", type=str)
args = parser.parse_args()

repo: Repository = Github(args.token).get_repo("subalterngames/fast_image_encoder")
release: GitRelease = repo.get_release(args.version)

# Create the release.
if release is None:
    release = repo.create_git_release(tag=args.version,
                                      name=args.version,
                                      message="TODO edit this",
                                      prerelease=False,
                                      target_commitish="main",
                                      draft=False)
    print(f"Created release: {args.version}")
# Upload.
release.upload_asset(path="libfast_image_encoder.so",
                     name="libfast_image_encoder.so",
                     content_type="application/x-msdownload")
print("Uploaded release.")
