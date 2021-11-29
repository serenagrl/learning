echo "`n***** Creating Secrets ..."
kubectl apply -f .\secret.yaml

echo "`n***** Creating ConfigMaps ..."
kubectl apply -f .\configmap.yaml

echo "`n***** Creating Pods and Services ..."
kubectl apply -f .\redis-statefulset.yaml

echo "`n***** Waiting for pods ..."
kubectl rollout status statefulsets redis

echo "`n***** Creating Redis Cluster ..."
kubectl exec -it redis-0 -- redis-cli --no-auth-warning -a p@ssw0rd --cluster create $(kubectl get pods -l app=redis-cluster -o jsonpath="{range.items[*]}{.status.podIP}:6379{'\n'}{end}") --cluster-replicas 1 --cluster-yes

echo "`n***** Listing all pods ..."
kubectl get pods -o wide

echo "`n***** Installation script executed!"