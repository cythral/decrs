# decrs

This is a simple secrets decryption tool written in Rust and uses AWS KMS to decrypt secrets.  This can be added to container images to decrypt secrets in the entrypoint / startup script and pass the decrypted values to a child process.  It will use the default credential lookup chain that the AWS CLI and SDKs use.

## Usage

```
decrs <base64 ciphertext from KMS>
```