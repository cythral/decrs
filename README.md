# decrs

This is a simple secrets decryption tool written in Rust and uses AWS KMS to decrypt secrets.  This can be added to container images to decrypt secrets in the entrypoint / startup script and pass the decrypted values to a child process.  It will use the default credential lookup chain that the AWS CLI and SDKs use.

## Installation
Decrs is a single static binary installed onto a Docker Image: `public.ecr.aws/cythral/decrs`

It can be installed into other images like so:
```
COPY --from=public.ecr.aws/cythral/decrs /decrs /usr/local/bin/decrs
```

This assumes that /usr/local/bin is in your $PATH.

## Usage

```
export PLAINTEXT=$(decrs <base64 ciphertext from KMS>)
```
