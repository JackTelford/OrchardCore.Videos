"""The slides of the Rate Limits video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

COST = [
    ("1", "Every request costs work", "The server does the work for each request, however many arrive."),
    ("2", "A flood slows everyone", "One client sending a flood of requests can slow the site for all visitors."),
    ("3", "Repeated attempts", "Guessing passwords needs many requests, and limits make them expensive."),
]

BUDGETS = [
    ("1", "Signed-in user", "Counted as that user, with a budget of their own."),
    ("2", "Anonymous visitor", "Counted by IP address."),
    ("3", "Shared address", "Visitors behind one IP address share one budget."),
]

OVER_BUDGET = [
    ("1", "Within the budget", "The request is served as usual."),
    ("2", "Over the budget", "The request is rejected, or waits if the limit has a queue."),
    ("3", "The response", "429 Too Many Requests, with no body."),
]

POLICY_TYPES = [
    ("1", "Global", "The dynamic requests of the whole site."),
    ("2", "Endpoint", "Requests for a path you enter and the paths below it, such as /blog."),
    ("3", "Group", "Endpoints that a feature has placed in a named group."),
    ("4", "Limiters", "Every policy holds limiters. A request must pass every limiter that applies."),
]

LIMITER_TYPES = [
    ("1", "Fixed window", "A set number of requests per time period, then the count resets."),
    ("2", "Sliding window", "The same kind of cap across a rolling period, so traffic is smoothed."),
    ("3", "Concurrency", "How many matching requests can run at the same time."),
    ("4", "Token bucket", "Short bursts allowed, with capacity refilled steadily."),
]

SHARED_ADDRESS = [
    ("1", "An office or a school", "Everyone reaches the internet through one gateway."),
    ("2", "A mobile carrier", "Many phones can share one public address."),
    ("3", "Signed-in users", "Each signed-in user keeps a budget of their own."),
]

PROXY = [
    ("1", "The address of the proxy", "Without the forwarded headers, every request seems to come from the proxy."),
    ("2", "One shared budget", "All anonymous visitors are counted against the same address."),
    ("3", "Trust only your proxies", "If you trust anyone's headers, clients can choose the address they are counted against."),
]

MOVING = [
    ("1", "Deployment plan", "The All Rate Limit Policies step exports every policy."),
    ("2", "Recipe", "A recipe can create or update policies when it runs."),
]


def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Floods of requests")
        bullets(image, COST)
        return image

    if step_id == "why-2":
        image = slide_base(chapter_title, label, "Who shares a budget")
        bullets(image, BUDGETS)
        return image

    if step_id == "why-3":
        image = slide_base(chapter_title, label, "What happens over the budget")
        bullets(image, OVER_BUDGET, active={1, 2})
        return image

    if step_id == "types-1":
        image = slide_base(chapter_title, label, "Three types of policy")
        bullets(image, POLICY_TYPES, active={0, 1})
        return image

    if step_id == "types-2":
        image = slide_base(chapter_title, label, "Three types of policy")
        bullets(image, POLICY_TYPES, active={2, 3})
        return image

    if step_id == "limiters-1":
        image = slide_base(chapter_title, label, "Four types of limiter")
        bullets(image, LIMITER_TYPES, active={0, 1})
        return image

    if step_id == "limiters-2":
        image = slide_base(chapter_title, label, "Four types of limiter")
        bullets(image, LIMITER_TYPES, active={2, 3})
        return image

    if step_id == "shared-1":
        image = slide_base(chapter_title, label, "Many visitors, one address")
        bullets(image, SHARED_ADDRESS)
        return image

    if step_id == "shared-2":
        image = slide_base(chapter_title, label, "Behind a reverse proxy")
        bullets(image, PROXY)
        return image

    if step_id == "move-1":
        image = slide_base(chapter_title, label, "Moving policies between sites")
        bullets(image, MOVING)
        return image

    raise SystemExit("No slide for " + step_id)
