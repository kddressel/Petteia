sailingwiththegods
===================

[![License: LGPL v3](https://img.shields.io/badge/License-LGPL%20v3-blue.svg)](LICENSE) 

[Visit the website](https://scholarblogs.emory.edu/samothraciannetworks) for more information!

# Setup

This Build uses Unity **2021.3.12f1**

unityhub://2021.3.12f1/8af3c3e441b1

Pasting that version link into a browser will auto-launch the installer in [Unity Hub](https://unity3d.com/get-unity/download), which we recommend.

## Setup Using SourceTree (recommended)
* Download and Install [SourceTree](https://www.sourcetreeapp.com/)
  * If asked, say you want embedded git, and you don't want mercurial
  * When asked if you want Bitbucket Server or Bitbucket, click "Skip"
  * So no when asked about loading an SSH key
* Add Github Account to SourceTree
  * Tools -> Options -> Authentication -> Add
  * Select Github for Hosting Service, leave everything else as it is (HTTPS, OAuth)
  * Click Refresh OAuth Token
  * Authenticate Github in the browser
* Clone the Repo
  * [Make your own fork](https://docs.github.com/en/get-started/quickstart/fork-a-repo) of the repository on the GitHub website
  * Click the Clone button **on your fork**
  * Copy paste the fork's HTTPS url into the URL field on the Clone screen
  * Choose "main" as the branch name in advanced
  * Choose manager-core on the popup that appear, and choose "Always use this"
* Add the kddressel repo as upstream remote
  * Click Repository -> Repository Settings
  * Click Add
  * Type "upstream" as the remote name, and paste https://github.com/kddressel/Petteia.git in as the URL, change to your github account for remote account and click OK
  * Fetch -> And make sure you fetch all remotes (this is the default setting)
  * If needed, right click upstream/branchname and merge into your fork's branch

## Git Setup for Windows

* Install [Git for Windows](https://git-scm.com/download/win) using **default settings**
  * Make sure you pick **"Git Credential Manager Core"**
* Use https athentication steps below to clone your fork

## Clone the project using https authentication

```
git clone yourforkurl 
git remote add upstream https://github.com/kddressel/Petteia.git

```

