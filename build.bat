@echo on
cls
.paket\paket install
packages\FAKE\tools\Fake.exe build.fsx buildType=%1 nugetDeployPath=%2
pause