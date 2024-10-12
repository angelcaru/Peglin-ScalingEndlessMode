TARGET_DEBUG=bin/Debug/netstandard2.1/ScalingEndlessMode.dll
TARGET_RELEASE=bin/Release/netstandard2.1/ScalingEndlessMode.dll

ALLCODE=Plugin.cs

$(TARGET_DEBUG): ScalingEndlessMode.csproj $(ALLCODE)
	dotnet build --configuration Debug
debug: $(TARGET_DEBUG)

$(TARGET_RELEASE): ScalingEndlessMode.csproj $(ALLCODE)
	dotnet build --configuration Release
release: $(TARGET_RELEASE)

ScalingEndlessMode.zip: $(TARGET_RELEASE) icon.png README.md manifest.json
	rm -f ScalingEndlessMode.zip
	zip -j ScalingEndlessMode.zip $(TARGET_RELEASE) icon.png README.md manifest.json
zip: ScalingEndlessMode.zip

all: debug release zip

install: $(TARGET_DEBUG)
	cp $(TARGET_DEBUG) ../BepInEx/plugins

instrelease: $(TARGET_RELEASE)
	cp $(TARGET_RELEASE) ../BepInEx/plugins

clean:
	rm -rf bin obj ScalingEndlessMode.zip

.PHONY: all debug release zip install instrelease clean
