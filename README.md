# dj.masayoshi.tv

This is a plug.dj-inspired webapp with Twitch integration, for sharing music.

## TODO/Roadmap

*Let's see how far I get with this*

- [x] Twitch auth & cookie sessions
- [ ] Basic homepage
- [ ] Basic room page
- [ ] Virtual actor system for rooms
- [ ] User YT link submission to queue (with video metadata fetching for timing, title etc.)
- [ ] Queue display (SSE)
- [ ] YT Media player (SSE updates)
- [ ] Queue/room moderation (Twitch mod list sync)
- [ ] Endpoint to get current media (for StreamElements/Fossabot !song commands etc.)
- [ ] ???
- [ ] ???
- [ ] ???
- [ ] Chat integration, submissions?


## Technical Breakdown

> [!NOTE]
> While this application is currently locked down to a single active room in production, it's being built with multiple rooms in mind.
> Design decisions like using the actor model wouldn't be made if it was truly a single-room app.
> And I may own a domain that would suit a real plug.dj clone...

The app is a hypermedia-driven application - making use of HTMX & ASP\.NET endpoints that serve up HTML.
Pure HTML & CSS for the front-end; no fancy client-side state management.

For the real-time element of this project, the virtual actor pattern (popularized by Orleans) is used through proto.actor.
Clients are notified of real-time updates with Server-Sent Events [SSE], and uses the HTMX SSE extension to display updates.

*I'll fill this out more once I've built a chunk of it, because you (yes you on GitHub!) are probably the only person reading it :)*

## License

This project is licensed under MPL 2.0.
See [LICENSE.txt](LICENSE.txt) for more information.
