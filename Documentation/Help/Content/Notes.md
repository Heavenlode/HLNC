# Server Debug Client
We need to implement an infinite scrolling mechanism.
Page size = `((scrollContainerWidth * 2) / frameWidth)`
1. OnScroll:
	1. If scrolling left, and scroll position is <50%
		1. Delete next page
		2. Load previous page
	2. If scrolling right, and scroll position is >50%
		1. Delete previous page
		2. if not live:
			1. Load next page
2. OnAddChild
	1. if pages loaded > 3
		1. Delete left page