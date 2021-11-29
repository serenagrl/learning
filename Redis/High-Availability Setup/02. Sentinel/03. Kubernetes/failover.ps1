$ServerName=$args[0]

if (!$ServerName)
{
	echo "Please provide a redis master server name."
	echo "Example usage: failover.ps1 redis-0"
	exit 1
}

echo "`n***** Setting sample data to $ServerName (will get an error if server is not a master) ..."
kubectl exec -it $ServerName -- redis-cli --no-auth-warning -a p@ssw0rd set greeting "Hello Sentinel"

echo "`n***** Get data from $ServerName ..."
kubectl exec -it $ServerName -- redis-cli --no-auth-warning -a p@ssw0rd get greeting 

echo "`n***** Get current master ..."
kubectl exec -it sentinel-0 -- redis-cli --raw -h sentinel -p 5000 sentinel get-master-addr-by-name redismaster

echo "`n***** Simulate failover ..."
kubectl exec -it sentinel-0 -- redis-cli -h sentinel -p 5000 sentinel failover redismaster

echo "`n***** Pause for while to wait for failover info to be updated ..."
Start-Sleep -s 2

echo "`n***** Get new master ..."
kubectl exec -it sentinel-0 -- redis-cli --raw -h sentinel -p 5000 sentinel get-master-addr-by-name redismaster

echo "`n***** Get data from $ServerName ..."
kubectl exec -it $ServerName -- redis-cli --no-auth-warning -a p@ssw0rd get greeting 

echo "`n***** Listing all pods ..."
kubectl get pods -o wide

echo "`n***** Test script executed!"
