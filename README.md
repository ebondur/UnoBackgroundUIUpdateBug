This Repo is a reproduction of a UI bug found while developing a production mobile application.

# Prereq's
- Android Device with NFC capabilities
- At least one NFC Tag to be scanned

# Description
Simply put, this bug occurs when a model is navigated to more than once and the UI updates orginate from a background thread. 
Upon navigating to the page the first time, all works as expected. When an NFC tag is read, the screen will update with the UID as expected.
This will work for however many tags are read while the page is still displayed for the first time. Since the methodology of capturing and marshaling
the NFC tag is done on a background thread, `await MainThread.InvokeOnMainThreadAsync` is called to push the UI updates back to the main thread.

To recreate the bug, navigate away from the page by hitting the back arrow on the Nav Bar, which will take you to back to the main page. 
Then, navigate back to the same previous page and scan the tag again. The UI will not update, no matter how many times the page is navigated to again.
This bug occurs for various other methods of UI Thread marshaling as well. 
