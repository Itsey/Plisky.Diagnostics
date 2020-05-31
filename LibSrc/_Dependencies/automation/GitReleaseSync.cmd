@Echo off
echo Sync With GitHub.
git tag -a vXXX-VERSION-XXX -m"Release Of Version XXX-VERSION-XXX"
git push origin
git push gh
