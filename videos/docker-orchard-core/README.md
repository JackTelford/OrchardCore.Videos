# Docker: Run Orchard Core in a Container

**YouTube:** not published yet · **Length:** 4:49 · **Feature:** `guide:topics/docker` ([Using Docker with Orchard Core](https://docs.orchardcore.net/en/latest/topics/docker/))

The video is for developers. It builds an image from the `Dockerfile` at the root of the Orchard Core repository, runs a
container and opens its Setup screen, shows the `App_Data` folder that a volume keeps outside the container, and uses
Docker Compose to start a PostgreSQL service next to the site. The commands, the Dockerfile and the Compose file are
slides; only the Setup screen of the container is recorded. The title, description, tags and chapters YouTube shows
are in `video.json`.

## Recording it again

The video is recorded on a **container that has never served a request**, so it needs no mask script, no sign-in and no
copy of an `App_Data` folder. `recording-plan.md` holds every value and every action.

1. Check out OrchardCore at the commit named in `recording-plan.md`, build the image with `docker build --platform=linux/amd64 -t oc .`
   at the root of the checkout, and start a container from it with `docker run --name oc-video -p 8080:80 oc`.
2. Do **not** set any Auto Setup variable and do not create a `state.json`: the Setup screen is the subject of the clip.
   Do not run `Recorder setup` or `Recorder prepare`.
3. Narrate and record: `scripts/narrate.py videos/docker-orchard-core`, then `Recorder record 04-first-request` with
   `VIDEO_BASE_URL=http://localhost:8080` and `VIDEO_PASSWORD` set to a password chosen outside git.
4. The clip installs the site. To record it again, remove the container (`docker rm -f oc-video`) and start another
   one from the same image. It is the only clip; the other chapters are slides.
5. Before recording, read `OPEN-QUESTIONS.md`: it lists what must be confirmed on a running container, including the
   Compose chapter, which has no recording of its own.
