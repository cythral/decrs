FROM public.ecr.aws/docker/library/rust:1.59.0-alpine3.15 as build
WORKDIR /app
ENV CFLAGS=-mno-outline-atomics
RUN rustup target add aarch64-unknown-linux-musl
RUN apk add --no-cache make perl make musl-dev

COPY . ./
RUN cargo build --target=aarch64-unknown-linux-musl --release
RUN strip target/aarch64-unknown-linux-musl/release/decrs

FROM scratch
COPY --from=build /app/target/aarch64-unknown-linux-musl/release/decrs /decrs
ENTRYPOINT [ "/decrs" ]