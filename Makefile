.PHONY: clean

malaco: Malaco\ 5.generated.sln
	dotnet build Malaco\ 5.generated.sln -o Malaco
	mkdir Malaco/db
	cp samples/status.txt Malaco/status.txt

clean: 
	rm -r Malaco
	rm -r bin