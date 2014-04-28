# Deprecation Notice
This sample project has been deprecated. It is no longer being actively maintained and is probably out of date.

In other words, if you decide to clone this repository, you're on your own.

# Google Mirror API's Quickstart for .NET
This project shows you how to implement a simple
piece of Glassware that demos the major functionality of the Google Mirror API.

To see a fully-working demo of the quick start project, go to
[https://glass-python-starter-demo.appspot.com](https://glass-java-starter-demo.appspot.com).
Otherwise, read on to see how to deploy your own version.

## Prerequisites

- .NET Framework 4.0 or higher.
- ASP .NET MVC 3 web framework or higher.
- A web server -  You need a place to host your files.
- To use subscriptions, you also need an Internet accessible hosting
  environment with a valid SSL certificate signed by a trusted certificate
  authority.

Note: You can start developing with a localhost instance of your HTTP server,
  but you must have an Internet accessible host to use the subscription features
  of the API.

## Configuring the project

Enter your client ID, client secret, and redirect URL in `Models/Config.cs`:

    internal static class Config
    {
        /// The OAuth2.0 Client ID of your project.
        /// </summary>
        public static readonly string CLIENT_ID = "[[YOUR_CLIENT_ID]]";

        /// <summary>
        /// The OAuth2.0 Client secret of your project.
        /// </summary>
        public static readonly string CLIENT_SECRET = "[[YOUR_CLIENT_SECRET]]";

        /// <summary>
        /// The OAuth2.0 scopes required by your project.
        /// </summary>
        public static readonly string[] SCOPES = new String[]
        {
            "https://www.googleapis.com/auth/glass.timeline",
            "https://www.googleapis.com/auth/glass.location",
            "https://www.googleapis.com/auth/userinfo.profile"
        };

        /// <summary>
        /// The Redirect URI of your project.
        /// </summary>
        public static readonly string REDIRECT_URI = "[[YOUR_REDIRECT_URI]]";
    }

## Running a local development server

Before deploying to your production server, you can deploy the Quick Start
project to a local development server for testing:

1. In Visual Studio, from the `Debug` menu, select `Start Debugging`.
2. Start using the Quick Start on your default browser at `http://localhost:61422`.


## Deploying the Quick Start project

Deploy the Quick Start project to your host server:

1. In **Solution Explorer**, right-click the project and click `Publish`.
2. Follow the instructions to complete the process.


