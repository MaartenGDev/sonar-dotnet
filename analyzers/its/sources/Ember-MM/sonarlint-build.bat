nuget restore "%~dp0Ember Media Manager.sln" -Verbosity quiet && msbuild /verbosity:detailed /m /t:rebuild /p:Configuration=Debug "%~dp0Ember Media Manager.sln"