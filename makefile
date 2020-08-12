up:
	make down
	docker-compose up -d

down:
	docker-compose down

test:
	make up
	dotnet test
	make down

pack:
	cd IdentityServer4.Contrib.DynamoDB
	dotnet pack -c Release --include-symbols --include-source --force