FROM public.ecr.aws/prima/rust:1.55.0-1 as build
WORKDIR /app
COPY . ./
RUN apt-get update
RUN apt-get install -y --no-install-recommends musl-tools
RUN rustup target add x86_64-unknown-linux-musl
RUN cargo build --target=x86_64-unknown-linux-musl --release
RUN strip target/x86_64-unknown-linux-musl/release/decrs

FROM scratch
COPY --from=build /app/target/x86_64-unknown-linux-musl/release/decrs /decrs
ENTRYPOINT [ "/decrs" ]