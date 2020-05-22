## What is this?

This is an initial repo for building a [KEDA](https://github.com/kedacore/keda) External Scaler based on .NET Core and gRPC. 

This is not a serious scaler, it allows Twitch audience in the chat to scale deployments in a cluster by sending the word "!kaboom", and scaling it down by "!feeeesst".

## How does it work?

External KEDA scalers have to implement a gRPC service represented in the file `Protos/externalscaler.proto`. This file originally can be found [here](https://github.com/kedacore/keda/blob/master/pkg/scalers/externalscaler/externalscaler.proto).

You have to build docker image for this scaler so you can deploy it to the cluster. So invoke `docker build . -t kaboom-scaler` to build the image, and then it will be ready to be used in the deployments below. If you don't want to build the image, we have built an image that can be used directly, but you have to change it in the deployment file below in the `image` attribute. 
You can find the image on Docker Hub: [eashi/kaboom:1.0.0](https://hub.docker.com/r/eashi/kaboom)

You can deploy the external scaler by runnning `kubectl deploy -f Deployments/kaboom-scaler-deployment.yaml`, and then expose it on an internal service by executing `kubectl deploy -f Deployments/kaboom-scaler-service.yaml`.

Once you deploy the scaler, you can configure the scaledobject that will tell KEDA to use this scaler to scale a specific deployment: `kubectl apply -f Deployments/scaledobject.yaml`. In this scaledobject resource you have three parameters that you have to provide:
- `scalerAddress`: which is the address of the service we created above, in our example it's `kaboom.default:80`.
- `accessToken`: this is the Twitch access token that you need to connect to the channel,  you can get this token from [https://twitchapps.com/tmi/](https://twitchapps.com/tmi/).
- `twitchUserName`: the twitch username to be used as the bot, in my case it's `emadashi`.
- `channelName`: is the channel name, which in my case it is the same as my user name `emadashi`.

You can choose which deployment to scale by configuring the parameter `deploymentName` in the same file.


## I want to join playing with this, what should I do?

This scaler is built during a live stream on https://twitch.tv/emadashi. Most of the time it is every Thursday (unitl it's done) at 20:00 Melbourne Australia time.

Come and join us, this is a relaxed streaming session we mistakes are allowed and contribution from the audience is highly encouraged :).

Ping me on twitter at [@emadashi](https://twitter.com/emadashi) if you have any questions or comments.

## Special Thanks
It wasn't just me who built this scaler, it's the great people who join my stream regurarly and make this streaming worthwhile!

Special thanks go to: 
- codeandcoffee ([@tkoster](https://github.com/tkoster))
- [@jsobell](https://github.com/jsobell)
 