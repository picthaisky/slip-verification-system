FROM fluent/fluentd:v1.16-1

USER root

# Install plugins
RUN gem install fluent-plugin-elasticsearch --no-document

USER fluent
