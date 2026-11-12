"""The slides of the Docker video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
The Dockerfile and the .dockerignore file are the ones at the root of the OrchardCMS/OrchardCore repository.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import W, bullets, draw_code, note_cards, slide_base

NEEDS = [
    ("1", "Windows", "Containers on the Windows Subsystem for Linux."),
    ("2", "Ubuntu and other Linux systems", "Docker Engine."),
]

WAYS = [
    ("1", "A published image", "orchardproject/orchardcore-cms-linux on Docker Hub."),
    ("2", "Your own image", "Built from the Dockerfile in the source code."),
]

RUN_PUBLISHED = """
docker run --name orchardcms -p 8080:80 orchardproject/orchardcore-cms-linux:latest
"""

RUN_PUBLISHED_OPTIONS = [
    ("1", "--name orchardcms", "Names the container."),
    ("2", "-p 8080:80", "Maps port 8080 of your machine to port 80 in the container."),
]

DOCKERFILE_BUILD = """
# TARGETARCH and TARGETOS are set automatically when --platform is provided.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:11.0 AS build-env
ARG TARGETOS
LABEL stage=build-env
WORKDIR /source

# copy required files for building
# .dockerignore excludes App_Data and binaries from these
COPY ./src ./src
COPY Directory.Build.props .
COPY Directory.Packages.props .

# build, results are placed in /app
RUN dotnet publish src/OrchardCore.Cms.Web/OrchardCore.Cms.Web.csproj \\
    -c Release -o /app --framework net11.0 /p:RunAnalyzers=false
"""

DOCKERFILE_RUNTIME = """
# build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:11.0-nanoserver-ltsc2025 AS build_windows
FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS build_linux
FROM build_${TARGETOS} AS aspnet

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
WORKDIR /app
COPY --from=build-env /app/ .
ENTRYPOINT ["dotnet", "OrchardCore.Cms.Web.dll"]
"""

DOCKERIGNORE = """
# Ignore any App_Data folder
**/App_Data/

# Ignore all built assets
**/[b|B]in/
**/[O|o]bj/
"""

OWN_SOLUTION = [
    ("1", "Copy two files", "The Dockerfile and .dockerignore, to the root of your solution."),
    ("2", "Adjust the paths", "The build stage follows the folder structure of the repository."),
]

COMMANDS = """
# Build the image
docker build --platform=linux/amd64 -t oc .

# Run a container
docker run -p 8080:80 oc

# Prune the intermediate images of the build stage
docker image prune -f --filter label=stage=build-env
"""

BUILD_OPTIONS = [
    ("1", "--platform=linux/amd64", "Sets TARGETOS, which selects the Linux runtime stage."),
    ("2", "-t oc", "Names the image oc."),
]

RUN_OPTIONS = [
    ("1", "-p 8080:80", "Maps port 8080 of your machine to port 80 in the container."),
]

PRUNE_OPTIONS = [
    ("1", "--filter label=stage=build-env", "Selects the intermediate images of the build stage."),
]

APP_DATA = [
    ("1", "/app/App_Data/tenants.json", "The tenants of the application."),
    ("2", "/app/App_Data/Sites/Default/OrchardCore.db", "The database of the Default tenant, when it uses Sqlite."),
    ("3", "/app/App_Data/Sites/Default/DataProtection-Keys", "The Data Protection keys of the Default tenant."),
]

VOLUME = """
docker run -p 8080:80 -v orchard-data:/app/App_Data oc
"""

VOLUME_OPTIONS = [
    ("1", "-v orchard-data:/app/App_Data", "Mounts the volume orchard-data on the App_Data folder."),
]

ENVIRONMENT = """
OrchardCore__AutoSetup__Tenants__0__ShellName=Default
OrchardCore__AutoSetup__Tenants__0__SiteName=Demo
OrchardCore__AutoSetup__Tenants__0__SiteTimeZone=America/Los_Angeles
OrchardCore__AutoSetup__Tenants__0__AdminUsername=admin
OrchardCore__AutoSetup__Tenants__0__AdminEmail=admin@example.com
# ...
"""

ENVIRONMENT_NOTES = [
    ("1", "Two underscores", "Separate the levels of a name: OrchardCore, AutoSetup, Tenants."),
    ("2", "Auto Setup", "Installs the site on the first request, with no Setup screen."),
]

COMPOSE = """
services:
  web:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5009:80"
    depends_on:
      - postgresql
  postgresql:
    image: postgres:latest
    volumes:
      - postgresql-data:/var/lib/postgresql
    environment:
      POSTGRES_USER: orchardcore_user
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: orchardcore_database
volumes:
  postgresql-data:
"""

CONNECTION_STRING = """
Server=postgresql;Port=5432;Database=orchardcore_database;User Id=orchardcore_user;Password=<password>
"""

CONNECTION_NOTES = [
    ("1", "What type of database to use?", "Choose Postgres."),
    ("2", "Connection String", "The server is the name of the database service."),
]

COMPOSE_COMMANDS = """
# Set the password of the database service
export POSTGRES_PASSWORD=<password>

# Build the web image, then start both containers
docker-compose build
docker-compose up
"""

COMPOSE_START_NOTES = [
    ("1", "Setup screen", "Open the mapped port, 5009, in a browser."),
]


def compose_panel(image, highlight=None):
    draw_code(image, (96, 240, 1010, 1000), "docker-compose.yml", "bash", COMPOSE, highlight=highlight, size=24)


def slide(step_id, chapter_title, label):
    if step_id == "need-1":
        image = slide_base(chapter_title, label, "What you need")
        bullets(image, NEEDS)
        return image

    if step_id == "idea-1":
        image = slide_base(chapter_title, label, "Two ways to get an image")
        bullets(image, WAYS)
        return image

    if step_id == "idea-2":
        image = slide_base(chapter_title, label, "Run the published image")
        draw_code(image, (96, 240, W - 96, 400), "Terminal", "bash", RUN_PUBLISHED, size=28)
        bullets(image, RUN_PUBLISHED_OPTIONS, top=460)
        return image

    if step_id == "dockerfile-1":
        image = slide_base(chapter_title, label, "The build stage")
        draw_code(image, (96, 240, W - 96, 900), "Dockerfile", "bash", DOCKERFILE_BUILD, size=26)
        return image

    if step_id == "dockerfile-2":
        image = slide_base(chapter_title, label, "The runtime stages")
        draw_code(image, (96, 240, W - 96, 800), "Dockerfile", "bash", DOCKERFILE_RUNTIME, size=26)
        return image

    if step_id == "dockerfile-3":
        image = slide_base(chapter_title, label, "Keep the data out of the image")
        draw_code(image, (96, 240, 1100, 660), ".dockerignore", "bash", DOCKERIGNORE, size=28)
        note_cards(image, [
            ("Site data", "App_Data stays out of the image."),
            ("Build output", "bin and obj stay out of it."),
        ], (1140, 300, W - 96, 1030))
        return image

    if step_id == "dockerfile-4":
        image = slide_base(chapter_title, label, "A site with its own solution")
        bullets(image, OWN_SOLUTION)
        return image

    if step_id == "build-1":
        image = slide_base(chapter_title, label, "Build the image")
        draw_code(image, (96, 240, W - 96, 640), "Terminal", "bash", COMMANDS, highlight={1, 2}, size=26)
        bullets(image, BUILD_OPTIONS, top=680)
        return image

    if step_id == "build-2":
        image = slide_base(chapter_title, label, "Run a container")
        draw_code(image, (96, 240, W - 96, 640), "Terminal", "bash", COMMANDS, highlight={4, 5}, size=26)
        bullets(image, RUN_OPTIONS, top=680)
        return image

    if step_id == "build-3":
        image = slide_base(chapter_title, label, "Prune the intermediate images")
        draw_code(image, (96, 240, W - 96, 640), "Terminal", "bash", COMMANDS, highlight={7, 8}, size=26)
        bullets(image, PRUNE_OPTIONS, top=680)
        return image

    if step_id == "data-1":
        image = slide_base(chapter_title, label, "What App_Data holds")
        bullets(image, APP_DATA)
        return image

    if step_id == "data-2":
        image = slide_base(chapter_title, label, "Keep the files in a volume")
        draw_code(image, (96, 240, W - 96, 400), "Terminal", "bash", VOLUME, size=28)
        bullets(image, VOLUME_OPTIONS, top=460)
        return image

    if step_id == "data-3":
        image = slide_base(chapter_title, label, "Settings as environment variables")
        draw_code(image, (96, 240, W - 96, 640), "Environment variables", "bash", ENVIRONMENT, size=26)
        bullets(image, ENVIRONMENT_NOTES, top=680)
        return image

    if step_id == "compose-1":
        image = slide_base(chapter_title, label, "A web service and a database")
        compose_panel(image)
        note_cards(image, [
            ("web", "Built from the Dockerfile."),
            ("postgresql", "The database of the site."),
        ], (1050, 300, W - 96, 1030))
        return image

    if step_id == "compose-2":
        image = slide_base(chapter_title, label, "The web service")
        compose_panel(image, highlight=set(range(1, 10)))
        note_cards(image, [
            ("build", "The Dockerfile in this folder."),
            ("ports", "Port 5009 to port 80."),
            ("depends_on", "The database service."),
        ], (1050, 300, W - 96, 1030))
        return image

    if step_id == "compose-3":
        image = slide_base(chapter_title, label, "The database service")
        compose_panel(image, highlight=set(range(10, 20)))
        note_cards(image, [
            ("environment", "User, database and password."),
            ("POSTGRES_PASSWORD", "Read from your environment."),
        ], (1050, 300, W - 96, 1030))
        return image

    if step_id == "compose-5":
        image = slide_base(chapter_title, label, "Connect the site to the database")
        draw_code(image, (96, 240, W - 96, 400), "Connection String", "bash", CONNECTION_STRING, size=24)
        bullets(image, CONNECTION_NOTES, top=460)
        return image

    if step_id == "compose-4":
        image = slide_base(chapter_title, label, "Start both containers")
        draw_code(image, (96, 240, W - 96, 640), "Terminal", "bash", COMPOSE_COMMANDS, size=26)
        bullets(image, COMPOSE_START_NOTES, top=680)
        return image

    raise SystemExit(f"No slide for {step_id}")
