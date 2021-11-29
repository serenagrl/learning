echo "`n***** Removing Redis Cluster ..."
kubectl delete -f .\redis-statefulset.yaml

echo "`n***** Removing ConfigMaps ..."
kubectl delete -f .\configmap.yaml

echo "`n***** Removing Secrets ..."
kubectl delete -f .\secret.yaml

echo "`n***** Removing Persistent Volume Claims ..."
kubectl delete pvc data-redis-0 data-redis-1 data-redis-2 data-redis-3 data-redis-4 data-redis-5

echo "`n***** Uninstallation script executed!"