module App

open Giraffe.GiraffeViewEngine
open Microsoft.AspNetCore.Http

let layout (ctx:HttpContext) (content: XmlNode list)  =
    html [_class "has-navbar-fixed-top"] [
        head [] [
            meta [_charset "utf-8"]
            meta [_name "viewport"; _content "width=device-width, initial-scale=1" ]
            title [] [encodedText "Hello IssueTracker.Web"]
            link [_rel "stylesheet"; _href "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css" ]
            link [_rel "stylesheet"; _href "https://cdnjs.cloudflare.com/ajax/libs/bulma/0.6.1/css/bulma.min.css" ]
            link [_rel "stylesheet"; _href "/app.css" ]
            link [_rel "stylesheet"; _href "/login.css" ]
            link [_rel "stylesheet"; _href "/admin.css" ]

        ]
        body [] [
            nav [ _class "navbar is-fixed-top has-shadow" ] [
                div [_class "navbar-brand"] [
                    a [_class "navbar-item"; _href "/"] [
                        str "Start"
                    ]
                    div [_class "navbar-burger burger"; attr "data-target" "navMenu"] [
                        span [] []
                        span [] []
                        span [] []
                    ]
                ]
                div [_class "navbar-menu"; _id "navMenu"] [
                    div [_class "navbar-start"] [
                        if (ctx.User.Identity.IsAuthenticated) then
                            a [_class "navbar-item"; _href "/projects"] [rawText "Project Management"]
                            a [_class "navbar-item"; _href "/users"] [rawText "User Management"]
                            a [_class "navbar-item"; _href "/logout"] [rawText "Logout"]
                    ]
                ]
            ]
            article [ _class "container" ] [
                yield! content
            ]
            
            footer [_class "footer is-fixed-bottom"] [
                div [_class "container"] [
                    div [_class "content has-text-centered"] [
                        p [] [
                            rawText "Created by "
                            a [_href "https://www.hardt-solutions.com"; _target "_blank"] [rawText "Hardt IT-Solutions"]                            
                        ]
                    ]
                ]
            ]
            script [_src "/app.js"] []
        ]
    ]
