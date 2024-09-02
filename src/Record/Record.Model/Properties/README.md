# commit.url
This file is over-written by the github deploy action to be the current commit url

For local building, it can be set correctly by
```bash
 echo https://github.com/equinor/records/commit/`git log --format="%H" -n 1` > commit.url
 ```