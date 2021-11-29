$ServerName=$args[0]

if (!$ServerName)
{
	echo "Please provide a redis server name to failover to."
	echo "Example usage: failover.ps1 redis-3"
	exit 1
}

echo "`n***** Listing all pods ..."
kubectl get pods -o wide

echo "`n***** Current cluster nodes ..."
kubectl exec -it $ServerName -- redis-cli --no-auth-warning -a p@ssw0rd -c cluster nodes

echo "`n***** Failover to node $ServerName ..."
kubectl exec -it $ServerName -- redis-cli --no-auth-warning -a p@ssw0rd -c cluster failover

Start-Sleep -s 2

echo "`n***** $ServerName now has the following replicas: "
$NodeId=kubectl exec -it $ServerName -- redis-cli --no-auth-warning -a p@ssw0rd -c cluster myid
kubectl exec -it $ServerName -- redis-cli --no-auth-warning -a p@ssw0rd -c cluster replicas $NodeId

echo "`n***** New cluster nodes ..."
kubectl exec -it $ServerName -- redis-cli --no-auth-warning -a p@ssw0rd -c cluster nodes

echo "`n***** Failover script executed!"