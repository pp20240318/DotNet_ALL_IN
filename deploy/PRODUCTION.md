# Production environment variables (reference only — do NOT commit real secrets)

Copy this file to your deployment platform's secret store or `.env` (gitignored).
Never commit files containing real production values.

## Admin API (WatchShop.Admin.Api)

| Variable / Config | Description |
|-------------------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Database__Server` | MySQL host |
| `Database__Password` | MySQL password |
| `Jwt__SecretKey` | Admin JWT signing key (min 32 chars) |
| `StoreJwt__SecretKey` | Store JWT signing key |
| `Payment__WebhookSecret` | Payment webhook HMAC secret |
| `Minio__AccessKey` / `Minio__SecretKey` | Object storage credentials |
| `Cors__AdminOrigins__0` | Admin web origin, e.g. `https://admin.example.com` |

## Store API (WatchShop.Store.Api)

Same database; uses `StoreJwt` section. Configure CORS if store web is on a separate domain.

## Frontends

| Variable | Description |
|----------|-------------|
| `VITE_API_BASE_URL` | `/__api` when nginx proxies; or full API URL |

Build-time variables — set in CI/CD or Docker build args.

## Docker Compose (local / staging)

Default credentials in `docker-compose.yml` are for **development only**.
For staging/production:

1. Override secrets via `docker compose --env-file production.env up`
2. Use external managed MySQL / Redis
3. Replace JWT secrets in mounted `appsettings.Production.json` or environment overrides

## Recommended checklist

- [ ] Rotate all default passwords (MySQL, MinIO, JWT)
- [ ] Enable HTTPS termination (reverse proxy / load balancer)
- [ ] Restrict Hangfire dashboard (`/hangfire`) to admin network
- [ ] Set `AllowedHosts` appropriately
- [ ] Configure log aggregation (Serilog sinks)
