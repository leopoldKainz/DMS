terraform {
    required_providers {
        aws = {
            source  = "hashicorp/aws"
            version = "~> 5.0"
        }
    }
}

variable "aws_access_key" {
    type = string
    sensitive = true
}
variable "aws_secret_key" {
    type = string
    sensitive = true
}
variable "aws_session_token" {
    type = string
    sensitive = true
}

provider "aws" {
    region     = "us-east-1"
    access_key = var.aws_access_key
    secret_key = var.aws_secret_key
    token      = var.aws_session_token
}

resource "tls_private_key" "temp_key" {
    algorithm = "RSA"
    rsa_bits  = 4096
}

resource "aws_key_pair" "deployer" {
    key_name   = "dms-deploy-key"
    public_key = tls_private_key.temp_key.public_key_openssh
}

resource "local_file" "ssh_key" {
    filename        = "${path.module}/deploy-key.pem"
    content         = tls_private_key.temp_key.private_key_pem
    file_permission = "0400"
}

resource "aws_security_group" "app_sg" {
    name        = "dms_app_sg"
    description = "Allow SSH and HTTP traffic"

    ingress {
        from_port   = 22
        to_port     = 22
        protocol    = "tcp"
        cidr_blocks = ["195.245.225.253/32"]
    }

    ingress {
        from_port   = 80
        to_port     = 80
        protocol    = "tcp"
        cidr_blocks = ["195.245.225.253/32"]
    }

    ingress {
        from_port   = 15672
        to_port     = 15672
        protocol    = "tcp"
        cidr_blocks = ["195.245.225.253/32"]
    }

    egress {
        from_port   = 0
        to_port     = 0
        protocol    = "-1"
        cidr_blocks = ["0.0.0.0/0"] 
    }
}

resource "aws_instance" "app_server" {
    ami           = "ami-0d97d7f07b25dd693"
    instance_type = "t3.small"
    key_name      = aws_key_pair.deployer.key_name
    vpc_security_group_ids = [aws_security_group.app_sg.id]

    tags = {
        Name = "DMS-App-Server"
    }
}

output "instance_ip" {
    value = aws_instance.app_server.public_ip
}