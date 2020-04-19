<template>
    <footer class="footer">
        <div class="content has-text-centered">
            <div class="soc">
                <span class="icon is-medium">
                    <a class="has-text-dark" aria-label="View my Github" href="https://github.com/lutzee">
                        <font-awesome-icon :icon="['fab', 'github']" size="2x" role="presentation" />
                    </a>
                </span>
                <span class="icon is-medium">
                    <a class="has-text-dark" aria-label="View my Twitter" href="https://twitter.com/lutzee_">
                        <font-awesome-icon :icon="['fab', 'twitter']" size="2x" role="presentation" />
                    </a>
                </span>
                <span class="icon is-medium">
                    <a class="has-text-dark" aria-label="View my LinkedIn" href="https://www.linkedin.com/in/lutzee/">
                        <font-awesome-icon :icon="['fab', 'linkedin']" size="2x" role="presentation" />
                    </a>
                </span>
                <span class="icon is-medium">
                    <a class="has-text-dark" aria-label="My email address contact@lutzee.net" href="mailto://contact@lutzee.net">
                        <font-awesome-icon :icon="['fas', 'at']" size="2x" role="presentation" />
                    </a>
                </span>
            </div>© 2020 Jamie Flint
        </div>
    </footer>
</template>

<script lang="ts">
    import { Component, Vue } from "vue-property-decorator";

    @Component
    export default class FooterVue extends Vue { }
</script>

<style lang="less">
    .sticky-footer {
        position: absolute;
        left: 0;
        bottom: 0;
        margin-left: auto;
        margin-right: auto;
        padding-top: 10px;
        padding-bottom: 10px;
    }
</style>
