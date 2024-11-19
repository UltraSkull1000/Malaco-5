.PHONY: clean

malaco: Malaco\ 5.generated.sln
	dotnet build Malaco\ 5.csproj -o build
	mkdir Malaco/db
	cp samples/status.txt build/status.txt

clean: 
	rm -r build