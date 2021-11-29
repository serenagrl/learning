# Redis High-Availability Setups

Contains all the assets and configurations to setup a Redis-Cluster or Redis-Sentinel environment for learning. **These setups are not required for running the Redis Learning Series code samples**. In fact, the code samples have not been tested with these setups. A separate sample webapi application is provided for each of these setup for testing.

## Container Environments

I used Docker and the Kubernetes provided by Docker. You can only spin-up one environment at a time (Docker, Kubernetes or Helm), but not all at once as they will cause port conflicts. If you are using other Kubernetes environment such as minikube or Kind, then you may need to modify some the settings to get things working.

## Folders

Each of the Cluster and Sentinel setups contain the following set of folders:

1. **Sample.WebApi**        - Sample application for testing (cache data).
2. **Docker**               - All assets and configuration needed to spin up the environment in Docker.
3. **Kubernetes**           - All assets and configuration needed to spin up the environment in Kubernetes.
4. **Helm**                 - All assets and configuration needed to spin up the environment using Helm.

## Sample Application

A sample application is provided for each Cluster and Sentinel environment. You will need to **build the docker image** for them in Visual Studio or use the Docker build command and then **deploy the image into Docker**.

You will also need to ensure that your **SQL Server database is acessible from within the Docker environment by configuring your firewall ports**.

The script to create the database and tables are located in `03. Database Folder`. Please run them on your SQL Server Database and **configure the connection string correctly for the Sample Application before you build the container image**.

You may also need to populate some random data into those tables for the testing purposes, otherwise, there is no data to cache.

## Docker Environment

Use docker compose in powershell to bring up or down the environment. Go to the folder where `docker-compose.yml` is located and then enter the following command in powershell. 

Bring up the environment and see the log outputs on the console.
```
docker compose up
```
Bring up the environment in the background.
```
docker compose up -d
```
Bring down the environment
```
docker compose down
```

The Sentinel environment configurations needs to be reseted to its initial state if you are trying to set it up from scratch again after bringing it down. You may copy the initial state configurations from the `Initial State` folder and replace your existing .conf files if you are re-setting up the Redis Sentinel again.

To run the Sample Application for Cluster,
```
docker run --rm --name clusterapp --net redis p- 18080:80 clustertestapp
```
To run the Sample Application for Sentinel,
```
docker run --rm --name sentinelapp --net redis p- 18080:80 sentineltestapp
```
To access the Sample Application, open up a browser and navigate to `http:\\localhost:18080\api\product`


## Kubernetes (Docker) Environment

The Kubernetes environment I tested this on was the one enabled through Docker (Just check the checkbox). Therefore, the storage class configuration was pre-configured by Docker. If you are using something else, please make sure you have a storage class configured to allow you to create persistent volumes.

To bring up the environment, you can use the provided `install.ps1` script.

To bring down and clean-up the environment, you can use the `uninstall.ps1` script instead.

The uninstall scripts are written to delete the persistence volume claims as well. If you did not use the uninstall scripts to bring down the environment, you will have to manually delete them yourself.

Both Cluster and Sentinel has a `failover.ps1` script for testing but the input for them are slightly different. The Sentinel failover script expects the master node name as argument, whereas, the Cluster failover script expects the name of the node you wish to failover to.

To run the Sample Application for Cluster or Sentinel, use the kubectl command on the `app-deployment.yaml` file in the `App` folder
```
kubectl apply -f app-deployment.yaml
```
To remove the sample application
```
kubectl delete -f app-deployment.yaml
```

>The pod initialization scripts for the Sentinel setup were taken and modified from [Marcel Demper's repository](https://github.com/marcel-dempers/docker-development-youtube-series/tree/master/storage/redis/kubernetes)


## Using Helm

Make sure you have helm installed and configured.

Run the `install.ps1` script to bring up the environment.

Run the `uninstall.ps1` script to bring down and clean-up the environment.

The uninstall scripts are written to delete the persistence volume claims as well. If you did not use the uninstall scripts to bring down the environment, you will have to manually delete them yourself.

**Note: I did not test the Sample Applications on Helm**


## Disclaimer

Everything is based on my own self learning and understanding. Please pardon any mistakes
and if you are taking these as a learning source, do be informed you are doing it at your own
risks. I have not tested these on production environments.
