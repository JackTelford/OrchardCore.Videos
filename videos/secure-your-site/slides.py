"""The slides of the Secure Your Site video. assemble.py calls slide() for each step of a "slide" clip.

Everything visual comes from brand.py, so the slides look like those of every other Orchard Core video.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

QUESTIONS = [
    ("1", "Who can do what?", "Access Control: Roles and Users"),
    ("2", "How do people sign in?", "Settings: User Login and Registration"),
    ("3", "How are connections and requests protected?", "Settings and Tools: HTTPS, headers, proxy, reCaptcha, rate limits, CORS"),
    ("4", "What is kept safe and recorded?", "Tools and Settings: Secrets and Audit Trail"),
]

CHECKLIST = [
    ("1", "Who can do what", "Keep Anonymous short. Disable accounts nobody uses."),
    ("2", "How people sign in", "Two-factor authentication for administrators. Verified email addresses, approval of registrations."),
    ("3", "How connections and requests are protected", "HTTPS, security headers, only the forwarded headers your proxy sends, reCaptcha, rate limits, a narrow CORS policy."),
    ("4", "What is kept safe and recorded", "Audit Trail events. Credentials kept as secrets."),
]


def slide(step_id, chapter_title, label):
    if step_id == "overview-1":
        image = slide_base(chapter_title, label, "Four questions to ask of your site")
        bullets(image, QUESTIONS)
        return image

    if step_id == "recap-1":
        image = slide_base(chapter_title, label, "The checklist")
        bullets(image, CHECKLIST, active={0, 1})
        return image

    if step_id == "recap-2":
        image = slide_base(chapter_title, label, "The checklist")
        bullets(image, CHECKLIST, active={2, 3})
        return image

    raise SystemExit("No slide for " + step_id)
